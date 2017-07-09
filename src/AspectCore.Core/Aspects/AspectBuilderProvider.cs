using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectBuilderProvider : IAspectBuilderProvider
    {
        private readonly IInterceptorProvider _interceptorProvider;

        public AspectBuilderProvider(IInterceptorProvider interceptorProvider)
        {
            if (interceptorProvider == null)
            {
                throw new ArgumentNullException(nameof(interceptorProvider));
            }
            _interceptorProvider = interceptorProvider;
        }

        public IAspectBuilder GetBuilder(AspectActivatorContext context)
        {
            var aspectBuilder = new AspectBuilder();
            foreach (var interceptor in _interceptorProvider.GetInterceptors(context.ServiceMethod))
            {
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }
            return aspectBuilder;
        }
    }
}
