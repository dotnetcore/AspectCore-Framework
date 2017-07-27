using System;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection.Internals
{
    internal sealed class AspectCoreBuilder : IAspectCoreBuilder
    {
        public IServiceCollection Services { get; }

        public AspectCoreBuilder(IServiceCollection services)
        {
            Services = services;
        }
    }
}
