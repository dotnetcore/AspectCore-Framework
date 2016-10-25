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
            services.AddScoped<IServiceProviderWrapper>(p =>
            {
                var ap = p as ProxyServiceProvider;
                if (ap == null) return new ServiceProviderWrapper(p);
                return new ServiceProviderWrapper(ap.originalServiceProvider);
            });
            services.AddSingleton<ModuleGenerator>();
            return services;
        }
    }
}
