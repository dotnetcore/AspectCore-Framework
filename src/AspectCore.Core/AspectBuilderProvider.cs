using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public sealed class AspectBuilderProvider : IAspectBuilderProvider
    {
        private readonly IInterceptorProvider _interceptorSelector;

        public AspectBuilderProvider(IInterceptorProvider interceptorSelector)
        {
            if (interceptorSelector == null)
            {
                throw new ArgumentNullException(nameof(interceptorSelector));
            }
            _interceptorSelector = interceptorSelector;
        }

        public IAspectBuilder GetBuilder(AspectActivatorContext context)
        {
            var aspectBuilder = new AspectBuilder();
            foreach (var interceptor in _interceptorSelector.GetInterceptors(context.ServiceMethod))
            {
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }
            return aspectBuilder;
        }
    }
}
