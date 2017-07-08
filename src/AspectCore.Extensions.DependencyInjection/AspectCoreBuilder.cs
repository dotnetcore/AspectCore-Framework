using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
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
