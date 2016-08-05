using AspectCore.Lite.Abstractions.Aspects;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAspects(this IServiceCollection services, Action<IAspectCollection, IAspectFactory> configure)
        {
            return AddAspects(services, null, null, configure);
        }

        public static IServiceCollection AddAspects(this IServiceCollection services, IAspectCollection aspectFactory, IAspectFactory factoryFactory, Action<IAspectCollection, IAspectFactory> configure)
        {

            if (services == null) throw new ArgumentNullException(nameof(services));

            IAspectCollection aspects = aspectFactory ?? new AspectCollection();
            IAspectFactory factory = factoryFactory;

            if (configure != null)
                configure(aspects, factory);

            services.AddSingleton<IAspectFactory>(factory);
            services.AddSingleton<IAspectCollection>(aspects);

            foreach (IAspect aspect in aspects)
            {

                //  services.AddTransient(aspect.);
            }

            return services;
        }
    }
}
