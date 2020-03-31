using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.DependencyInjection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class InterceptorCollector : IInterceptorCollector
    {
        private readonly IEnumerable<IInterceptorSelector> _interceptorSelectors;
        private readonly IEnumerable<IAdditionalInterceptorSelector> _additionalInterceptorSelectors;
        private readonly IPropertyInjectorFactory _propertyInjectorFactory;
        private readonly IAspectCaching _aspectCaching;

        public InterceptorCollector(
            IEnumerable<IInterceptorSelector> interceptorSelectors,
            IEnumerable<IAdditionalInterceptorSelector> additionalInterceptorSelectors,
            IPropertyInjectorFactory propertyInjectorFactory,
            IAspectCachingProvider aspectCachingProvider)
        {
            if (interceptorSelectors == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelectors));
            }
            if (additionalInterceptorSelectors == null)
            {
                throw new ArgumentNullException(nameof(additionalInterceptorSelectors));
            }
            if (propertyInjectorFactory == null)
            {
                throw new ArgumentNullException(nameof(propertyInjectorFactory));
            }
            if (aspectCachingProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectCachingProvider));
            }
            _interceptorSelectors = interceptorSelectors.Distinct(new InterceptorSelectorEqualityComparer<IInterceptorSelector>()).ToList();
            _additionalInterceptorSelectors = additionalInterceptorSelectors.Distinct(new InterceptorSelectorEqualityComparer<IAdditionalInterceptorSelector>()).ToList();
            _propertyInjectorFactory = propertyInjectorFactory;
            _aspectCaching = aspectCachingProvider.GetAspectCaching(nameof(InterceptorCollector));
        }

        public IEnumerable<IInterceptor> Collect(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            if (serviceMethod == null)
            {
                throw new ArgumentNullException(nameof(serviceMethod));
            }
            if (implementationMethod == null)
            {
                throw new ArgumentNullException(nameof(implementationMethod));
            }

            return (IEnumerable<IInterceptor>)_aspectCaching.GetOrAdd(GetKey(serviceMethod, implementationMethod), key =>
            {
               return HandleInjector(CollectFromService(serviceMethod).
                    Concat(CollectFromAdditionalSelector(serviceMethod, implementationMethod)).
                    HandleSort().
                    HandleMultiple()).Distinct().ToArray();
            });
        }

        private object GetKey(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            return Tuple.Create(serviceMethod, implementationMethod);
        }

        private IEnumerable<IInterceptor> CollectFromService(MethodInfo serviceMethod)
        {
            var inherited = CollectFromInherited(serviceMethod);
            var selected = CollectFromSelector(serviceMethod);
            return inherited.Concat(selected);
        }

        private IEnumerable<IInterceptor> CollectFromInherited(MethodInfo method)
        {
            var typeInfo = method.DeclaringType.GetTypeInfo();
            var interceptors = new List<IInterceptor>();
            if (!typeInfo.IsClass)
            {
                return interceptors;
            }
            foreach (var interfaceType in typeInfo.GetInterfaces())
            {
                var interfaceMethod = interfaceType.GetTypeInfo().GetDeclaredMethodBySignature(new MethodSignature(method));
                if (interfaceMethod != null)
                {
                    interceptors.AddRange(CollectFromService(interfaceMethod).Where(x => x.Inherited));
                }
            }
            interceptors.AddRange(CollectFromBase(method));
            return interceptors;
        }

        private IEnumerable<IInterceptor> CollectFromBase(MethodInfo method)
        {
            var typeInfo = method.DeclaringType.GetTypeInfo();
            var interceptors = new List<IInterceptor>();
            var baseType = typeInfo.BaseType;
            if (baseType == typeof(object) || baseType == null)
            {
                return interceptors;
            }
            var baseMethod = baseType.GetTypeInfo().GetMethodBySignature(new MethodSignature(method));
            if (baseMethod != null)
            {
                interceptors.AddRange(CollectFromBase(baseMethod).Where(x => x.Inherited));
                interceptors.AddRange(CollectFromSelector(baseMethod).Where(x => x.Inherited));
            }

            return interceptors;
        }

        private IEnumerable<IInterceptor> CollectFromSelector(MethodInfo method)
        {
            foreach (var selector in _interceptorSelectors)
            {
                foreach (var interceptor in selector.Select(method))
                {
                    if (interceptor != null)
                        yield return interceptor;
                }
            }
        }

        private IEnumerable<IInterceptor> CollectFromAdditionalSelector(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            foreach (var selector in _additionalInterceptorSelectors)
            {
                foreach (var interceptor in selector.Select(serviceMethod, implementationMethod))
                {
                    if (interceptor != null)
                        yield return interceptor;
                }
            }
        }

        private IEnumerable<IInterceptor> HandleInjector(IEnumerable<IInterceptor> interceptors)
        {
            foreach (var interceptor in interceptors)
            {
                if (PropertyInjectionUtils.Required(interceptor))
                {
                    _propertyInjectorFactory.Create(interceptor.GetType()).Invoke(interceptor);
                }
                yield return interceptor;
            }
        }
    }

    internal static class InterceptorCollectorExtensions
    {
        internal static IEnumerable<IInterceptor> HandleMultiple(this IEnumerable<IInterceptor> interceptors)
        {
            var set = new HashSet<Type>();
            foreach (var interceptor in interceptors)
            {
                if (interceptor.AllowMultiple)
                {
                    yield return interceptor;
                    continue;
                }
                if (set.Add(interceptor.GetType()))
                {
                    yield return interceptor;
                }
            }
        }

        internal static IEnumerable<IInterceptor> HandleSort(this IEnumerable<IInterceptor> interceptors)
        {
            return interceptors.OrderBy(x => x.Order);
        }
    }
}