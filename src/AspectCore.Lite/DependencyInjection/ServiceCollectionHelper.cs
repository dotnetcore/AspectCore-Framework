using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Internal;
using AspectCore.Lite.Internal.Generators;
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
            services.AddTransient<IPointcut, Pointcut>();
            services.AddTransient<IProxyActivator , ServiceProxyActivator>();
            services.AddTransient<IServiceScope>(
                provider => provider.GetRequiredService<IServiceScopeFactory>().CreateScope());
            services.AddSingleton<ModuleGenerator>();
            return services;
        }
    }
}
