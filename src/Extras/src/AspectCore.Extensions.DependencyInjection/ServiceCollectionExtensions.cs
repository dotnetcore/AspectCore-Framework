using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDynamicProxy(this IServiceCollection services, Action<IAspectConfiguration> configure = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var configurationService = services.LastOrDefault(x => x.ServiceType == typeof(IAspectConfiguration) && x.ImplementationInstance != null);
            var configuration = (IAspectConfiguration)configurationService?.ImplementationInstance ?? new AspectConfiguration();
            configure?.Invoke(configuration);

            if (configurationService == null)
            {
                services.AddSingleton<IAspectConfiguration>(configuration);
            }

            services.AddTransient(typeof(IManyEnumerable<>), typeof(ManyEnumerable<>));
            services.AddScoped<IServiceResolver, MSDIServiceResolver>();
            services.AddScoped<IPropertyInjectorFactory, PropertyInjectorFactory>();          
            services.AddScoped<IInterceptorCollector, InterceptorCollector>();         
            services.AddScoped<IAspectContextFactory, AspectContextFactory>();   
            services.AddScoped<IAspectActivatorFactory, AspectActivatorFactory>();
            services.AddScoped<IProxyGenerator, ProxyGenerator>();      
            services.AddScoped<IParameterInterceptorSelector, ParameterInterceptorSelector>();
            services.AddSingleton<IInterceptorSelector, ConfigureInterceptorSelector>();
            services.AddSingleton<IInterceptorSelector, TypeInterceptorSelector>();
            services.AddSingleton<IInterceptorSelector, MethodInterceptorSelector>();
            services.AddSingleton<IAspectValidatorBuilder, AspectValidatorBuilder>();
            services.AddSingleton<IAspectBuilderFactory, AspectBuilderFactory>();
            services.AddSingleton<IProxyTypeGenerator, ProxyTypeGenerator>();
            services.AddSingleton<IAspectCachingProvider, AspectCachingProvider>();

            return services;
        }
    }
}