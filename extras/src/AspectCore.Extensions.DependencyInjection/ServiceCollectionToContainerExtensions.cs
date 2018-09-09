using System;
using System.Linq;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionToContainerExtensions
    {
        public static IServiceProvider BuildAspectInjectorProvider(this IServiceCollection services)
        {
            return BuildAspectInjectorProvider(services, null);
        }

        public static IServiceProvider BuildAspectInjectorProvider(this IServiceCollection services, Action<IServiceContainer> additional)
        {
            var container = services.ToServiceContainer();
            additional?.Invoke(container);
            return container.Build();
        }

        public static IServiceContainer ToServiceContainer(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<ISupportRequiredService, SupportRequiredService>();
            services.AddScoped<IServiceScopeFactory, ServiceScopeFactory>();
            return new ServiceContainer(services.AddAspectCoreContainer().Select(Replace));
        }

        public static IServiceCollection AddAspectCoreContainer(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IServiceProviderFactory<IServiceContainer>, AspectCoreServiceProviderFactory>();
            return services;
        }

        private static ServiceDefinition Replace(ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                return new TypeServiceDefinition(descriptor.ServiceType, descriptor.ImplementationType, GetLifetime(descriptor.Lifetime));
            }
            else if (descriptor.ImplementationInstance != null)
            {
                return new InstanceServiceDefinition(descriptor.ServiceType, descriptor.ImplementationInstance);
            }
            else
            {
                return new DelegateServiceDefinition(descriptor.ServiceType, resolver => descriptor.ImplementationFactory(resolver), GetLifetime(descriptor.Lifetime));
            }
        }

        private static Lifetime GetLifetime(ServiceLifetime serviceLifetime)
        {
            switch (serviceLifetime)
            {
                case ServiceLifetime.Scoped:
                    return Lifetime.Scoped;
                case ServiceLifetime.Singleton:
                    return Lifetime.Singleton;
                default:
                    return Lifetime.Transient;
            }
        }
    }
}