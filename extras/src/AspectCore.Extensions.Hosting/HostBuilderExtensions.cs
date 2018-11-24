using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspectCore.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureAspectInjector(this IHostBuilder hostBuilder, Action<IServiceContainer> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.ConfigureAspectInjector();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder ConfigureAspectInjector(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceContainer> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.ConfigureAspectInjector();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder ConfigureAspectInjector(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(new AspectCoreServiceProviderFactory());
        }

        public static IHostBuilder ConfigureDynamicProxy(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection, IAspectConfiguration> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            if (configureDelegate == null)
            {
                throw new ArgumentNullException(nameof(configureDelegate));
            }
            hostBuilder.UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());

            hostBuilder.ConfigureServices((host, services) =>
            {
                services.AddDynamicProxy(config =>
                {
                    configureDelegate(host, services, config);
                });
            });

            return hostBuilder;
        }

        public static IHostBuilder ConfigureDynamicProxy(this IHostBuilder hostBuilder, Action<IServiceCollection, IAspectConfiguration> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureServices(services =>
                {
                    services.AddDynamicProxy(config =>
                    {
                        configureDelegate(services, config);
                    });
                });
            }
            return hostBuilder;
        }
    }
}