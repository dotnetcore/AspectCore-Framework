using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public sealed class ScopeAspectContextFactory : AspectContextFactory
    {
        private readonly IAspectScheduler _aspectContextScheduler;

        public ScopeAspectContextFactory(IServiceProvider serviceProvider, IAspectScheduler aspectContextScheduler) : base(serviceProvider)
        {
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        public override AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            return new ScopeAspectContext(base.CreateContext(activatorContext), _aspectContextScheduler);
        }
    }
}
