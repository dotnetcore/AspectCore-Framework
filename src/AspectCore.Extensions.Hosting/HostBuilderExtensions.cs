using System;
using AspectCore.Configuration;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspectCore.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseServiceContext(this IHostBuilder hostBuilder, Action<IServiceContext> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseServiceContext();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder UseServiceContext(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceContext> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseServiceContext();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder UseServiceContext(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(new ServiceContextProviderFactory());
        }
        
        public static IHostBuilder UseDynamicProxy(this IHostBuilder hostBuilder)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseServiceProviderFactory(new DynamicProxyServiceProviderFactory());
            return hostBuilder;
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

            hostBuilder.ConfigureServices((host, services) =>
            {
                services.ConfigureDynamicProxy(config =>
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
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureServices(services =>
                {
                    services.ConfigureDynamicProxy(config =>
                    {
                        configureDelegate(services, config);
                    });
                });
            }
            return hostBuilder;
        }
    }
}