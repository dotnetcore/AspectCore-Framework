using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using AspectCore.Lite.Extensions;
using AspectCore.Lite.Internal;
using AspectCore.Lite.Internal.Generators;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceProvider BuildAspectServiceProvider(this IServiceCollection services)
        {
            ExceptionHelper.ThrowArgumentNull(services, nameof(services));
            var serviceProvider = services.TryAddAspectCoreLite().BuildServiceProvider();
            var aspectServices = new ServiceCollection();
            services.ForEach(descriptor =>
            {
                if (CanProxy(descriptor, serviceProvider))
                    aspectServices.Add(CreateProxy(descriptor, serviceProvider));
                else
                    aspectServices.Add(descriptor);
            });
            aspectServices.AddScoped<IOriginalServiceProvider>(_ =>
                new OriginalServiceProvider(serviceProvider.GetRequiredService<IServiceScope>().ServiceProvider));
            return aspectServices.BuildServiceProvider();
        }

        public static IServiceCollection TryAddAspectCoreLite(this IServiceCollection services)
        {
            ExceptionHelper.ThrowArgumentNull(services, nameof(services));
            services.TryAddTransient<IJoinPoint, JoinPoint>();
            services.TryAddTransient<IAspectExecutor, AspectExecutor>();
            services.TryAddTransient<IServiceScope>(
                serviceProvider => serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope());
            services.TryAddScoped<IAspectContextFactory, AspectContextFactory>();
            services.TryAddScoped<IPropertyInjector, PropertyInjector>();
            services.TryAddSingleton<IInjectedPropertyMatcher, InjectedPropertyMatcher>();
            services.TryAddSingleton<IInterceptorMatcher, AttributeInterceptorMatcher>();
            services.TryAddSingleton<INamedMethodMatcher, NamedMethodMatcher>();
            services.TryAddSingleton<IPointcut, Pointcut>();
            services.TryAddSingleton<ModuleGenerator>();
            services.TryAddSingleton<IInterceptorCollection>(new InterceptorCollection());
            return services;
        }

        public static IServiceCollection AddInterceptors(this IServiceCollection services, Action<IInterceptorCollection> action)
        {
            ExceptionHelper.ThrowArgumentNull(services, nameof(services));
            action?.Invoke(GetInterceptorCollection(services));
            return services;
        }

        private static Boolean CanProxy(ServiceDescriptor serviceDescriptor, IServiceProvider serviceProvider)
        {
            var implementationType = serviceDescriptor.ImplementationType;
            if (implementationType == null)
            {
                return false;
            }
            if (!serviceDescriptor.ServiceType.GetTypeInfo().CanProxy(serviceProvider))
            {
                return false;
            }

            if (!serviceDescriptor.ImplementationType.GetTypeInfo().CanInherited())
            {
                return false;
            }
            return true;
        }

        private static ServiceDescriptor CreateProxy(ServiceDescriptor serviceDescriptor, IServiceProvider serviceProvider)
        {
            var proxyGenerator = new ClassProxyGenerator(serviceProvider, serviceDescriptor.ServiceType,
                               serviceDescriptor.ImplementationType,
                               serviceDescriptor.ImplementationType.GetTypeInfo().GetInterfaces());

            return ServiceDescriptor.Describe(serviceDescriptor.ServiceType,
                                proxyGenerator.GenerateProxyType(), serviceDescriptor.Lifetime);
        }

        private static IInterceptorCollection GetInterceptorCollection(IServiceCollection services)
        {
            var serviceDescriptor = services.FirstOrDefault(d => d.ServiceType == typeof(IInterceptorCollection) && d.ImplementationInstance != null);
            if (serviceDescriptor == null)
            {
                serviceDescriptor = ServiceDescriptor.Singleton<IInterceptorCollection>(new InterceptorCollection());
                services.TryAdd(serviceDescriptor);
            }
            return (IInterceptorCollection)serviceDescriptor.ImplementationInstance;
        }
    }
}