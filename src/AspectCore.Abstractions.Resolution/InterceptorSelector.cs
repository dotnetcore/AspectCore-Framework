using AspectCore.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class InterceptorSelector : IInterceptorSelector
    {
        private static readonly IDictionary<MethodInfo, IInterceptor[]> InterceptorCache = new Dictionary<MethodInfo, IInterceptor[]>();
        private static readonly object CacheLock = new object();

        private readonly IInterceptorMatcher interceptorMatcher;
        private readonly IInterceptorInjectorProvider interceptorInjectorProvider;

        public InterceptorSelector(
          IInterceptorMatcher interceptorMatcher,
          IInterceptorInjectorProvider interceptorInjectorProvider)
        {
            if (interceptorMatcher == null)
            {
                throw new ArgumentNullException(nameof(interceptorMatcher));
            }
            if (interceptorInjectorProvider == null)
            {
                throw new ArgumentNullException(nameof(interceptorInjectorProvider));
            }
            this.interceptorMatcher = interceptorMatcher;
            this.interceptorInjectorProvider = interceptorInjectorProvider;
        }

        public IInterceptor[] Select(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return InterceptorCache.GetOrAdd(method, _ =>
            {
                return interceptorMatcher.Match(method, method.DeclaringType.GetTypeInfo()).
                 Select(i =>
                 {
                     interceptorInjectorProvider.GetInjector(i.GetType()).Inject(i);
                     return i;
                 }).ToArray();
            }, CacheLock);
        }
    }
}