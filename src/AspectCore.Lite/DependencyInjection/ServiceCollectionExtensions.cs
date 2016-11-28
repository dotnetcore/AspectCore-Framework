using System;
using System.Reflection;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Extensions;
using AspectCore.Lite.Internal;
using AspectCore.Lite.Internal.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider BuildAspectServiceProvider(this IServiceCollection services)
        {
            var serviceProvider = services.AddAspectCoreLite().BuildServiceProvider();
            var aspectServices = new ServiceCollection();

            foreach (var serviceDescriptor in services)
            {
                var implementationType = serviceDescriptor.ImplementationType;
                if (implementationType != null)
                {
                    if (serviceDescriptor.ServiceType.GetTypeInfo().CanProxy(serviceProvider))
                    {
                        if (serviceDescriptor.ImplementationType.GetTypeInfo().CanInherited())
                        {
                            var proxyGenerator = new ClassProxyGenerator(serviceProvider,
                                serviceDescriptor.ServiceType,
                                implementationType,
                                implementationType.GetTypeInfo().GetInterfaces());

                            var proxyType = proxyGenerator.GenerateProxyType();

                            aspectServices.Add(ServiceDescriptor.Describe(serviceDescriptor.ServiceType, proxyType,
                                serviceDescriptor.Lifetime));

                            continue;
                        }
                    }
                }
                aspectServices.Add(serviceDescriptor);
            }

            aspectServices.AddScoped<IOriginalServiceProvider>(
                _ =>
                    new OriginalServiceProvider(
                        serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope().ServiceProvider));

            return aspectServices.BuildServiceProvider();
        }

        private static IServiceCollection AddAspectCoreLite(this IServiceCollection services)
        {
            services.AddTransient<IJoinPoint, JoinPoint>();
            services.AddScoped<IAspectContextFactory, AspectContextFactory>();
            services.AddTransient<IAspectExecutor, AspectExecutor>();
            services.AddSingleton<IInterceptorMatcher , AttributeInterceptorMatcher>();
            services.AddSingleton<INamedMethodMatcher , NamedMethodMatcher>();
            services.AddSingleton<IPointcut, Pointcut>();
            services.AddSingleton<ModuleGenerator>();
            services.AddTransient<IServiceScope>(
                serviceProvider => serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope());
            return services;
        }


    }
}