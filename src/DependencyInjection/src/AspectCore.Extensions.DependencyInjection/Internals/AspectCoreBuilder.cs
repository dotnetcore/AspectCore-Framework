using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal sealed class AspectCoreBuilder : IAspectCoreBuilder
    {
        public IServiceCollection Services { get; }

        public AspectCoreBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            Services = services;
        }
    }
}
