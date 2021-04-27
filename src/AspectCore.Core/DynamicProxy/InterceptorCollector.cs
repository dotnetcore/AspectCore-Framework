using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.DependencyInjection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截器收集器,提供方法获取服务和实例上关联的所有拦截器
    /// </summary>
    [NonAspect]
    public sealed class InterceptorCollector : IInterceptorCollector
    {
        private readonly IEnumerable<IInterceptorSelector> _interceptorSelectors;
        private readonly IEnumerable<IAdditionalInterceptorSelector> _additionalInterceptorSelectors;
        private readonly IPropertyInjectorFactory _propertyInjectorFactory;
        private readonly IAspectCaching _aspectCaching;

        /// <summary>
        /// 拦截器收集器,提供方法获取服务和实例上关联的所有拦截器
        /// </summary>
        /// <param name="interceptorSelectors">查询器集合</param>
        /// <param name="additionalInterceptorSelectors">IAdditionalInterceptorSelector集合</param>
        /// <param name="propertyInjectorFactory">属性注入工厂</param>
        /// <param name="aspectCachingProvider">缓存提供者</param>
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

        /// <summary>
        /// 获取服务和实例上关联的所有拦截器
        /// </summary>
        /// <param name="serviceMethod">服务方法</param>
        /// <param name="implementationMethod">目标方法</param>
        /// <returns>拦截器集合</returns>
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