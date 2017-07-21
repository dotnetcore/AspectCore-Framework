using System;
using AspectCore.Extensions.Configuration;
using AspectCore.Extensions.DependencyInjection.Internals;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IAspectCoreBuilder AddAspectCore(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            return AddAspectCore(services, null);
        }

        public static IAspectCoreBuilder AddAspectCore(this IServiceCollection services, Action<AspectCoreOptions> options)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var builder = new AspectCoreBuilder(services);

            builder.AddAspectActivator();
            builder.AddAspectContext();
            builder.AddAspectValidator();
            builder.AddInterceptorActivator();
            builder.AddInterceptorInjector();
            builder.AddInterceptorProvider();
            builder.AddAspectConfigure(options);
          
            return builder;
        }

        public static IServiceProvider BuildAspectCoreServiceProvider(this IServiceCollection services)
        {
            return new AspectCoreServiceProviderFactory().CreateServiceProvider(services);
        }
    }
}
