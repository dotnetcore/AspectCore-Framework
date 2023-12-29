using System;
using System.Linq;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionToServiceContextExtensions
    {
        public static IServiceProvider BuildServiceContextProvider(this IServiceCollection services)
        {
            return BuildServiceContextProvider(services, null);
        }

        public static IServiceProvider BuildServiceContextProvider(this IServiceCollection services, Action<IServiceContext> additional)
        {
            var container = services.ToServiceContext();
            additional?.Invoke(container);
            return container.Build();
        }

        public static IServiceContext ToServiceContext(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddScoped<ISupportRequiredService, SupportRequiredService>();
            services.AddScoped<IServiceScopeFactory, ServiceScopeFactory>();
            return new ServiceContext(services.AddAspectServiceContext().Select(Replace));
        }

        public static IServiceCollection AddAspectServiceContext(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.TryAddSingleton<IServiceProviderFactory<IServiceContext>, ServiceContextProviderFactory>();
            return services;
        }

        private static ServiceDefinition Replace(ServiceDescriptor descriptor)
        {
#if NET8_0_OR_GREATER
            if (descriptor.IsKeyedService)
            {
                if (descriptor.KeyedImplementationType != null)
                {
                    return new TypeServiceDefinition(descriptor.ServiceType, descriptor.KeyedImplementationType, GetLifetime(descriptor.Lifetime));
                }
                else if (descriptor.KeyedImplementationInstance != null)
                {
                    return new InstanceServiceDefinition(descriptor.ServiceType, descriptor.KeyedImplementationInstance);
                }
                else
                {
                    return new DelegateServiceDefinition(descriptor.ServiceType, resolver => descriptor.KeyedImplementationFactory(resolver, descriptor.ServiceKey), GetLifetime(descriptor.Lifetime));
                }
            }
#endif
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