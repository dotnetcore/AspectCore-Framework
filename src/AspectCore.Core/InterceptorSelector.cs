using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Abstractions.Internal;

namespace AspectCore.Core
{
    public sealed class InterceptorSelector : IInterceptorProvider
    {
        private static readonly IDictionary<MethodInfo, IInterceptor[]> interceptorCache = new Dictionary<MethodInfo, IInterceptor[]>();

        private readonly IInterceptorSelector _interceptorMatcher;
        private readonly IInterceptorInjectorProvider _interceptorInjectorProvider;

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
            this._interceptorMatcher = interceptorMatcher;
            this._interceptorInjectorProvider = interceptorInjectorProvider;
        }

        public IInterceptor[] GetInterceptors(MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return interceptorCache.GetOrAdd(method, _ =>
            {
                return _interceptorMatcher.Select(method, method.DeclaringType.GetTypeInfo()).
                 Select(i =>
                 {
                     _interceptorInjectorProvider.GetInjector(i.GetType()).Inject(i);
                     return i;
                 }).ToArray();
            });
        }
    }
}