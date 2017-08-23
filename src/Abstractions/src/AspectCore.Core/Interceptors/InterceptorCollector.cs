using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class InterceptorCollector : IInterceptorCollector
    {
        private static readonly ConcurrentDictionary<MethodInfo, IEnumerable<IInterceptor>> interceptorCache = new ConcurrentDictionary<MethodInfo, IEnumerable<IInterceptor>>();

        private readonly IEnumerable<IInterceptorSelector> _interceptorSelectors;
        private readonly IPropertyInjectorFactory _propertyInjectorFactory;

        public InterceptorCollector(IEnumerable<IInterceptorSelector> interceptorSelectors, IPropertyInjectorFactory propertyInjectorFactory)
        {
            if (interceptorSelectors == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelectors));
            }
            if (propertyInjectorFactory == null)
            {
                throw new ArgumentNullException(nameof(propertyInjectorFactory));
            }
            _interceptorSelectors = interceptorSelectors;
            _propertyInjectorFactory = propertyInjectorFactory;
        }

        public IEnumerable<IInterceptor> Collect(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return interceptorCache.GetOrAdd(method, m =>
            {
                var inherited = CollectFromInherited(m);
                var selected = CollectFromSelector(m);
                var collection = HandleInjector(selected).Concat(inherited).HandleSort().HandleMultiple();
                return collection.ToArray();
            });
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
                    list.AddRange(Collect(interfaceMethod).Where(x => x.Inherited));
                }
            }
            var baseType = typeInfo.BaseType;
            if (baseType == typeof(object))
            {
                return list;
            }
            var baseMethod = baseType.GetTypeInfo().GetMethod(new MethodSignature(method));
            if (baseMethod != null)
            {
                list.AddRange(Collect(baseMethod).Where(x => x.Inherited));
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
            foreach (var interceptor in interceptors)
            {
                _propertyInjectorFactory.Create(interceptor.GetType()).Invoke(interceptor);
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