using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Core
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
