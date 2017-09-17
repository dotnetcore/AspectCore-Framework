using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.ScopedContext
{
    internal sealed class ScopedAspectContextFactory : AspectContextFactory
    {
        private readonly IAspectContextScheduler _aspectContextScheduler;

        public ScopedAspectContextFactory(IServiceProvider serviceProvider, IAspectContextScheduler aspectContextScheduler) : base(serviceProvider)
        {
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        public override AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            return new ScopedAspectContext(base.CreateContext(activatorContext), _aspectContextScheduler);
        }
    }
}
