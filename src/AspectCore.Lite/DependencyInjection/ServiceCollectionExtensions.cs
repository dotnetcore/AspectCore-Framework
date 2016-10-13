using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Generators;
using AspectCore.Lite.Internal;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using AspectCore.Lite.Extensions;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IEnumerable<ServiceDescriptor> AddAspectLite(this IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }

            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IJoinPoint, JoinPoint>();
            services.AddTransient<IAspectContextFactory, AspectContextFactory>();
            services.AddSingleton<EmitBuilderProvider>();
            services.AddTransient<IAspectExecutor , AspectExecutor>();
            services.ForEach(d => serviceCollection.TryAdd(d));

            return services;
        }
    }
}
