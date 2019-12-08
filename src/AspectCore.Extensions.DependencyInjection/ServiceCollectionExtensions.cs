using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        [Obsolete("Use ConfigureDynamicProxy")]
        public static IServiceCollection AddDynamicProxy(this IServiceCollection services, Action<IAspectConfiguration> configure = null)
        {
            return ConfigureDynamicProxy(services, configure);
        }

        public static IServiceCollection ConfigureDynamicProxy(this IServiceCollection services, Action<IAspectConfiguration> configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var configurationService = services.LastOrDefault(x => x.ServiceType == typeof(IAspectConfiguration) && x.ImplementationInstance != null);
            var configuration = (IAspectConfiguration) configurationService?.ImplementationInstance ?? new AspectConfiguration();
            configure?.Invoke(configuration);

            if (configurationService == null)
            {
                services.AddSingleton(configuration);
            }

            return services;
        }

        internal static IServiceCollection TryAddDynamicProxyServices(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.ConfigureDynamicProxy();

            services.TryAddTransient(typeof(IManyEnumerable<>), typeof(ManyEnumerable<>));
            services.TryAddScoped<IServiceResolver, MsdiServiceResolver>();
            services.TryAddScoped<IScopeResolverFactory, MsdiScopeResolverFactory>();
            services.TryAddScoped<IPropertyInjectorFactory, PropertyInjectorFactory>();
            services.TryAddScoped<IAspectContextFactory, AspectContextFactory>();
            services.TryAddScoped<IAspectActivatorFactory, AspectActivatorFactory>();
            services.TryAddScoped<IProxyGenerator, ProxyGenerator>();
            services.TryAddScoped<IParameterInterceptorSelector, ParameterInterceptorSelector>();

            services.TryAddSingleton<IInterceptorCollector, InterceptorCollector>();
            services.TryAddSingleton<IAspectValidatorBuilder, AspectValidatorBuilder>();
            services.TryAddSingleton<IAspectBuilderFactory, AspectBuilderFactory>();
            services.TryAddSingleton<IProxyTypeGenerator, ProxyTypeGenerator>();
            services.TryAddSingleton<IAspectCachingProvider, AspectCachingProvider>();
            services.TryAddSingleton<IAspectExceptionWrapper, AspectExceptionWrapper>();

            services.AddSingleton<IInterceptorSelector, ConfigureInterceptorSelector>();
            services.AddSingleton<IInterceptorSelector, AttributeInterceptorSelector>();
            services.AddSingleton<IAdditionalInterceptorSelector, AttributeAdditionalInterceptorSelector>();

            return services;
        }
    }
}