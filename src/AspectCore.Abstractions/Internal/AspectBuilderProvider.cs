using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Internal
{
    public sealed class AspectBuilderProvider : IAspectBuilderProvider
    {
        private readonly IInterceptorProvider interceptorSelector;

        public AspectBuilderProvider(IInterceptorProvider interceptorSelector)
        {
            if (interceptorSelector == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelector));
            }
            this.interceptorSelector = interceptorSelector;
        }

        public IAspectBuilder GetBuilder(AspectActivatorContext context)
        {
            var aspectBuilder = new AspectBuilder();
            foreach (var interceptor in interceptorSelector.GetInterceptors(context.ServiceMethod))
            {
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }
            return aspectBuilder;
        }
    }
}
