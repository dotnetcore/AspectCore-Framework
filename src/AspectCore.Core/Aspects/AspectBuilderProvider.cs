using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectBuilderProvider : IAspectBuilderProvider
    {
        private readonly IInterceptorProvider _interceptorProvider;
        private readonly IAspectContextScheduler _aspectContextScheduler;

        public AspectBuilderProvider(IInterceptorProvider interceptorProvider, IAspectContextScheduler aspectContextScheduler)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        public IAspectBuilder GetBuilder(AspectContext context)
        {
            var aspectBuilder = new AspectBuilder();

            foreach (var interceptor in _interceptorProvider.GetInterceptors(context.Target.ServiceMethod))
                if (_aspectContextScheduler.TryInclude(context as ScopedAspectContext, interceptor))
                    aspectBuilder.AddAspectDelegate(interceptor.Invoke);

            return aspectBuilder;
        }
    }
}
