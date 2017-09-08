using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Extensions.DependencyInjection
{
    public static class ServiceCollectionBuildExtensions
    {
        public static IServiceProvider BuildAspectCoreServiceProvider(this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            var serviceProvider = services.BuildServiceProvider();

            var serviceValidator = new ServiceValidator(serviceProvider.GetRequiredService<IAspectValidatorBuilder>());
            var proxyTypeGenerator = serviceProvider.GetRequiredService<IProxyTypeGenerator>();

            var dynamicProxyServices = new ServiceCollection();

            foreach (var service in services)
            {
                if (serviceValidator.TryValidate(service, out Type implementationType))
                    dynamicProxyServices.Add(MakeProxyService(service, implementationType, proxyTypeGenerator));
                else
                    dynamicProxyServices.Add(service);
            }

            serviceProvider.Dispose();

            return dynamicProxyServices.BuildServiceProvider();
        }

        private static ServiceDescriptor MakeProxyService(ServiceDescriptor descriptor, Type implementationType, IProxyTypeGenerator proxyTypeGenerator)
        {
            if (descriptor.ServiceType.GetTypeInfo().IsInterface)
            {
                var proxyType = proxyTypeGenerator.CreateInterfaceProxyType(descriptor.ServiceType, implementationType);
                return ServiceDescriptor.Describe(
                  descriptor.ServiceType,
                  CreateFactory(descriptor, proxyType),
                  descriptor.Lifetime);
            }
            else
            {
                return ServiceDescriptor.Describe(
                    descriptor.ServiceType,
                    proxyTypeGenerator.CreateClassProxyType(descriptor.ServiceType, implementationType),
                    descriptor.Lifetime);
            }
        }

        private static Func<IServiceProvider, object> CreateFactory(ServiceDescriptor descriptor, Type proxyType)
        {
            var proxyConstructor = proxyType.GetTypeInfo().GetConstructor(new Type[] { typeof(IAspectActivatorFactory), descriptor.ServiceType });
            var reflector = proxyConstructor.GetReflector();
            if (descriptor.ImplementationInstance != null)
            {
                var implementationInstance = descriptor.ImplementationInstance;
                return provider => reflector.Invoke(provider.GetRequiredService<IAspectActivatorFactory>(), implementationInstance);
            }
            else if (descriptor.ImplementationFactory != null)
            {
                var implementationFactory = descriptor.ImplementationFactory;
                return provider => reflector.Invoke(provider.GetRequiredService<IAspectActivatorFactory>(), implementationFactory(provider));
            }
            else
            {
                var implementationType = descriptor.ImplementationType;
                return provider =>
                {
                    var aspectActivatorFactory = provider.GetRequiredService<IAspectActivatorFactory>();
                    var instance = ActivatorUtilities.CreateInstance(provider, implementationType);
                    return reflector.Invoke(aspectActivatorFactory, instance);
                };
            }
        }
    }
}