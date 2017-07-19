using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IInterceptorProvider _interceptorProvider;

        public AspectBuilderFactory(IInterceptorProvider interceptorProvider)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
        }

        public IAspectBuilder Create(AspectContext context)
        {
            var aspectBuilder = new AspectBuilder();

            foreach (var interceptor in _interceptorProvider.GetInterceptors(context.Target.ServiceMethod))
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);

            return aspectBuilder;
        }
    }
}