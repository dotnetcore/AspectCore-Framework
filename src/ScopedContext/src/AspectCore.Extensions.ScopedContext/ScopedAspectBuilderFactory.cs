using System;
using AspectCore.Abstractions;
using AspectCore.Core;

namespace AspectCore.Extensions.ScopedContext
{
    [NonAspect]
    public sealed class ScopedAspectBuilderFactory : IAspectBuilderFactory
    {
        private readonly IInterceptorProvider _interceptorProvider;
        private readonly IAspectContextScheduler _aspectContextScheduler;

        public ScopedAspectBuilderFactory(IInterceptorProvider interceptorProvider, IAspectContextScheduler aspectContextScheduler)
        {
            _interceptorProvider = interceptorProvider ?? throw new ArgumentNullException(nameof(interceptorProvider));
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        public IAspectBuilder Create(AspectContext context)
        {
            var aspectBuilder = new AspectBuilder();

            foreach (var interceptor in _interceptorProvider.GetInterceptors(context.Target.ServiceMethod))
                if (_aspectContextScheduler.TryInclude(context as ScopedAspectContext, interceptor))
                    aspectBuilder.AddAspectDelegate(interceptor.Invoke);

            return aspectBuilder;
        }
    }
}
