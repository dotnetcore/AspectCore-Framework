using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class InterceptorProvider : IInterceptorProvider
    {
        private static readonly IDictionary<MethodInfo, IEnumerable<IInterceptor>> interceptorCache = new Dictionary<MethodInfo, IEnumerable<IInterceptor>>();

        private readonly IEnumerable<IInterceptorSelector> _interceptorSelectors;
        private readonly IInterceptorInjectorProvider _interceptorInjectorProvider;

        public InterceptorProvider(IEnumerable<IInterceptorSelector> interceptorSelectors, IInterceptorInjectorProvider interceptorInjectorProvider)
        {
            if (interceptorSelectors == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelectors));
            }
            if (interceptorInjectorProvider == null)
            {
                throw new ArgumentNullException(nameof(interceptorInjectorProvider));
            }
            _interceptorSelectors = interceptorSelectors;
            _interceptorInjectorProvider = interceptorInjectorProvider;
        }

        public IEnumerable<IInterceptor> GetInterceptors(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return interceptorCache.GetOrAdd(method, _ => RemovalMultiple(SelectInterceptors(_)).OrderBy(x => x.Order).ToArray());
        }

        private IEnumerable<IInterceptor> SelectInterceptors(MethodInfo method)
        {
            foreach (var selector in _interceptorSelectors)
            {
                foreach (var interceptor in selector.Select(method, method.DeclaringType.GetTypeInfo()))
                {
                    _interceptorInjectorProvider.GetInjector(interceptor.GetType()).Inject(interceptor);
                    yield return interceptor;
                }
            }
        }

        private IEnumerable<IInterceptor> RemovalMultiple(IEnumerable<IInterceptor> interceptors)
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
    }
}