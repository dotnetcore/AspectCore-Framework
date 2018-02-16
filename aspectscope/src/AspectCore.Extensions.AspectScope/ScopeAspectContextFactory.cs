using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public sealed class ScopeAspectContextFactory : IAspectContextFactory
    {
        private readonly IAspectScheduler _aspectScheduler;
        private readonly AspectContextFactory _aspectContextFactory;

        public ScopeAspectContextFactory(IServiceProvider serviceProvider, IAspectScheduler aspectContextScheduler)
        {
            _aspectScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
            _aspectContextFactory = new AspectContextFactory(serviceProvider);
        }

        public AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            var aspectContext = _aspectContextFactory.CreateContext(activatorContext);
            if (!_aspectScheduler.TryEnter(aspectContext))
            {
                throw new InvalidOperationException("Error occurred in the schedule AspectContext.");
            }
            return aspectContext;
        }

        public void ReleaseContext(AspectContext aspectContext)
        {
            _aspectContextFactory.ReleaseContext(aspectContext);
            _aspectScheduler.Release(aspectContext);
        }
    }
}