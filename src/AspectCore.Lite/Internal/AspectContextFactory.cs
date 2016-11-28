using AspectCore.Lite.Abstractions;
using System;

namespace AspectCore.Lite.Internal
{
    internal sealed class AspectContextFactory : IAspectContextFactory
    {
        private readonly IServiceProvider serviceProvider;
        public AspectContextFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IAspectContext Create()
        {
            return new AspectContext(serviceProvider);
        }
    }
}
