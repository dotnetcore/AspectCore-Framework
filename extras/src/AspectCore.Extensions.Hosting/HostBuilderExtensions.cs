using System;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AspectCore.Extensions.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder ConfigureAspectCoreContainer(this IHostBuilder hostBuilder, Action<IServiceContainer> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseAspectCore();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder ConfigureAspectCore(this IHostBuilder hostBuilder, Action<IServiceCollection> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseAspectCore();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder ConfigureAspectCoreContainer(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceContainer> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseAspectCore();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder ConfigureAspectCore(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection> configureDelegate)
        {
            if (hostBuilder == null)
            {
                throw new ArgumentNullException(nameof(hostBuilder));
            }
            hostBuilder.UseAspectCore();
            if (configureDelegate != null)
            {
                hostBuilder.ConfigureContainer(configureDelegate);
            }
            return hostBuilder;
        }

        public static IHostBuilder UseAspectCore(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseServiceProviderFactory(new AspectCoreServiceProviderFactory());
        }
    }
}