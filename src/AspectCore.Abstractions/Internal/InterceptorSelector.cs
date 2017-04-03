using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Internal
{
    public sealed class InterceptorSelector : IInterceptorProvider
    {
        private static readonly IDictionary<MethodInfo, IInterceptor[]> InterceptorCache = new Dictionary<MethodInfo, IInterceptor[]>();

        private readonly IInterceptorSelector interceptorMatcher;
        private readonly IInterceptorInjectorProvider interceptorInjectorProvider;

        public InterceptorSelector(
          IInterceptorSelector interceptorMatcher,
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

        public IInterceptor[] GetInterceptors(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return InterceptorCache.GetOrAdd(method, _ =>
            {
                return interceptorMatcher.Select(method, method.DeclaringType.GetTypeInfo()).
                 Select(i =>
                 {
                     interceptorInjectorProvider.GetInjector(i.GetType()).Inject(i);
                     return i;
                 }).ToArray();
            });
        }
    }
}