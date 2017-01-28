using AspectCore.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class AspectBuilderProvider : IAspectBuilderProvider
    {
        private readonly static IDictionary<MethodInfo, IAspectBuilder> AspectBuilderCache = new Dictionary<MethodInfo, IAspectBuilder>();
        private readonly static object CacheLock = new object();

        private readonly IInterceptorSelector interceptorSelector;

        public AspectBuilderProvider(IInterceptorSelector interceptorSelector)
        {
            if (interceptorSelector == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelector));
            }
            this.interceptorSelector = interceptorSelector;
        }

        public IAspectBuilder GetBuilder(AspectActivatorContext context)
        {
            return AspectBuilderCache.GetOrAdd(context.ServiceMethod, key =>
            {
                var aspectBuilder = new AspectBuilder();
                foreach (var interceptor in interceptorSelector.Select(key))
                {
                    aspectBuilder.AddAspectDelegate(interceptor.Invoke);
                }
                return aspectBuilder;
            }, CacheLock);
        }
    }
}
