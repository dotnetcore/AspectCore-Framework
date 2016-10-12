using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Generators;
using AspectCore.Lite.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAspectLite(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            serviceCollection.AddTransient<IJoinPoint, JoinPoint>();
            serviceCollection.AddTransient<IAspectContextFactory, AspectContextFactory>();
            serviceCollection.AddSingleton<EmitBuilderProvider>();
            serviceCollection.AddTransient<IAspectExecutor , AspectExecutor>();

            return serviceCollection;
        }
    }
}
