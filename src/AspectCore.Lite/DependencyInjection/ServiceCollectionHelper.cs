using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Generators;
using AspectCore.Lite.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceCollectionHelper
    {
        public static IServiceCollection CreateAspectLiteServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddTransient<IJoinPoint, JoinPoint>();
            services.AddTransient<IAspectContextFactory, AspectContextFactory>();       
            services.AddTransient<IAspectExecutor, AspectExecutor>();
            services.AddTransient<IInterceptorMatcher , AttributeInterceptorMatcher>();
            services.AddTransient<INamedMethodMatcher , NamedMethodMatcher>();
            services.AddTransient<IPointcut, DefaultPointcut>();
            services.AddTransient<IProxyActivator , ProxyActivatorWrapper>();
            services.AddTransient<IServiceProviderWrapper>(provider =>
            {
                var proxyServiceProvider = provider as ProxyServiceProvider;
                if (proxyServiceProvider == null) return new ServiceProviderWrapper(provider);
                return new ServiceProviderWrapper(proxyServiceProvider.originalServiceProvider);
            });
            services.AddSingleton<ModuleGenerator>();
            return services;
        }
    }
}
