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
        public static IEnumerable<ServiceDescriptor> GetAspectLiteServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IJoinPoint, JoinPoint>();
            services.AddTransient<IAspectContextFactory, AspectContextFactory>();
            services.AddSingleton<EmitBuilderProvider>();
            services.AddTransient<IAspectExecutor, AspectExecutor>();
            services.AddScoped<IServiceProviderWrapper>(p =>
            {
                var ap = p as ProxyServiceProvider;
                if (ap == null) return new ServiceProviderWrapper(p);
                return new ServiceProviderWrapper(ap.originalServiceProvider);
            });
            return services;
        }

        public static IServiceCollection AddAspectLite(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            GetAspectLiteServices().ForEach(d => services.TryAdd(d));

            return services;
        }
    }
}
