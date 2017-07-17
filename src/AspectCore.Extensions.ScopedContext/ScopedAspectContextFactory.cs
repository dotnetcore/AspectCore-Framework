using System;
using AspectCore.Abstractions;
using AspectCore.Core;

namespace AspectCore.Extensions.ScopedContext
{
    public sealed class ScopedAspectContextFactory : AspectContextFactory
    {
        private readonly IAspectContextScheduler _aspectContextScheduler;

        public ScopedAspectContextFactory(IServiceProvider serviceProvider, IAspectContextScheduler aspectContextScheduler) : base(serviceProvider)
        {
            _aspectContextScheduler = aspectContextScheduler ?? throw new ArgumentNullException(nameof(aspectContextScheduler));
        }

        public override AspectContext CreateContext<TReturn>(AspectActivatorContext activatorContext)
        {
            return new ScopedAspectContext(base.CreateContext<TReturn>(activatorContext), _aspectContextScheduler);
        }
    }
}
