using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer ToServiceContainer(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            return new ServiceContainer(services.Select(x => Replace(x)));
        }

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

            foreach(var service in configuration.ServiceContainer)
            {
                services.Add(Replace(service));
            }

            services.AddTransient(typeof(IManyEnumerable<>), typeof(ManyEnumerable<>));
            services.AddScoped<IServiceResolver, MSDIServiceResolver>();

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

        private static ServiceDescriptor Replace(ServiceDefinition definition)
        {
            if (definition is TypeServiceDefinition typeServiceDefinition)
            {
                return ServiceDescriptor.Describe(typeServiceDefinition.ServiceType, typeServiceDefinition.ImplementationType, GetLifetime(typeServiceDefinition.Lifetime));
            }
            else if (definition is InstanceServiceDefinition instanceServiceDefinition)
            {
                return ServiceDescriptor.Singleton(instanceServiceDefinition.ServiceType, instanceServiceDefinition.ImplementationInstance);
            }
            else if (definition is DelegateServiceDefinition delegateServiceDefinition)
            {
                return new DelegateServiceDefinition(delegateServiceDefinition.ServiceType, provider =>
                {
                    var resolver=provider.get
                }, GetLifetime(delegateServiceDefinition.Lifetime));
            }
            throw new NotImplementedException();
        }

        private static ServiceLifetime GetLifetime(Lifetime lifetime)
        {
            switch (lifetime)
            {
                case Lifetime.Scoped:
                    return ServiceLifetime.Scoped;
                case Lifetime.Singleton:
                    return ServiceLifetime.Singleton;
                default:
                    return ServiceLifetime.Transient;
            }
        }
    }
}