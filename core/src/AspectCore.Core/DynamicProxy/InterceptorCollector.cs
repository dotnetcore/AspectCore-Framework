using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.Injector;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    public sealed class InterceptorCollector : IInterceptorCollector
    {
        private readonly IEnumerable<IInterceptorSelector> _interceptorSelectors;
        private readonly IPropertyInjectorFactory _propertyInjectorFactory;
        private readonly IAspectCaching _aspectCaching;

        public InterceptorCollector(
            IEnumerable<IInterceptorSelector> interceptorSelectors,
            IPropertyInjectorFactory propertyInjectorFactory,
            IAspectCachingProvider aspectCachingProvider)
        {
            if (interceptorSelectors == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelectors));
            }
            if (propertyInjectorFactory == null)
            {
                throw new ArgumentNullException(nameof(propertyInjectorFactory));
            }
            if (aspectCachingProvider == null)
            {
                throw new ArgumentNullException(nameof(aspectCachingProvider));
            }
            _interceptorSelectors = interceptorSelectors.Distinct(new InterceptorSelectorEqualityComparer()).ToList();
            _propertyInjectorFactory = propertyInjectorFactory;
            _aspectCaching = aspectCachingProvider.GetAspectCaching(nameof(InterceptorCollector));
        }

        public IEnumerable<IInterceptor> Collect(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return (IEnumerable<IInterceptor>)_aspectCaching.GetOrAdd(method, key =>
            {
                return HandleInjector(CollectInternal(method));
            });
        }

        private IEnumerable<IInterceptor> CollectInternal(MethodInfo method)
        {
            var inherited = CollectFromInherited(method);
            var selected = CollectFromSelector(method);
            var collection = selected.Concat(inherited).HandleSort().HandleMultiple();
            return collection.ToArray();
        }

        private IEnumerable<IInterceptor> CollectFromInherited(MethodInfo method)
        {
            var typeInfo = method.DeclaringType.GetTypeInfo();
            var list = new List<IInterceptor>();
            if (!typeInfo.IsClass)
            {
                return list;
            }
            foreach (var interfaceType in typeInfo.GetInterfaces())
            {
                var interfaceMethod = interfaceType.GetTypeInfo().GetDeclaredMethod(new MethodSignature(method));
                if (interfaceMethod != null)
                {
                    list.AddRange(CollectInternal(interfaceMethod).Where(x => x.Inherited));
                }
            }
            var baseType = typeInfo.BaseType;
            if (baseType == typeof(object) || baseType == null)
            {
                return list;
            }
            var baseMethod = baseType.GetTypeInfo().GetMethod(new MethodSignature(method));
            if (baseMethod != null)
            {
                list.AddRange(CollectInternal(baseMethod).Where(x => x.Inherited));
            }
            return list;
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

        private IEnumerable<IInterceptor> HandleInjector(IEnumerable<IInterceptor> interceptors)
        {
            foreach (var interceptor in interceptors.Where(x => PropertyInjectionUtils.Required(x)))
            {
                _propertyInjectorFactory.Create(interceptor.GetType()).Invoke(interceptor);
            }
            return interceptors;
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