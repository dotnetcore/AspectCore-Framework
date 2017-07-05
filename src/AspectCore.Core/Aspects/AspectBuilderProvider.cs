using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
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

        public IAspectBuilder GetBuilder(Abstractions.AspectContext context)
        {
            var aspectBuilder = new AspectBuilder();
            foreach (var interceptor in _interceptorProvider.GetInterceptors(context.Target.ServiceMethod))
            {
                aspectBuilder.AddAspectDelegate(interceptor.Invoke);
            }
            return aspectBuilder;
        }
    }
}
