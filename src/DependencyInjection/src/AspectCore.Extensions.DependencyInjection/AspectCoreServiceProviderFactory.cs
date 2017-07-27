using System;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;
using AspectCore.Extensions.DependencyInjection.Internals;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Extensions.DependencyInjection
{
    public class AspectCoreServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {

        private static readonly MethodInfo GetImplementationType = typeof(ServiceDescriptor).GetTypeInfo().GetMethod("GetImplementationType", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly MethodReflector GetImplementationTypeAccessor = new MethodReflector(GetImplementationType);

        public IServiceCollection CreateBuilder(IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }
            var serviceProvider = services.BuildServiceProvider();

            var aspectValidator = serviceProvider.GetRequiredService<IAspectValidatorBuilder>().Build();

            var dynamicProxyServices = new ServiceCollection();

            var generator = serviceProvider.GetRequiredService<IProxyGenerator>();

            foreach (var descriptor in services)
            {
                Type proxyType;
                if (!Validate(descriptor, aspectValidator, out Type implementationType))
                {
                    dynamicProxyServices.Add(descriptor);
                    continue;
                }

                if (descriptor.ServiceType.GetTypeInfo().IsInterface)
                {
                    if (services.Count(x => x.ServiceType == descriptor.ServiceType) > 1)
                    {
                        proxyType = generator.CreateClassProxyType(implementationType, implementationType);
                    }
                    else
                    {
                        proxyType = generator.CreateInterfaceProxyType(descriptor.ServiceType, implementationType);
                    }
                }
                else
                {
                    proxyType = generator.CreateClassProxyType(descriptor.ServiceType, implementationType);
                }
                dynamicProxyServices.Add(ServiceDescriptor.Describe(descriptor.ServiceType, proxyType, descriptor.Lifetime));
                ServiceInstanceProvider.MapServiceDescriptor(descriptor);
            }

            dynamicProxyServices.AddScoped<IRealServiceProvider>(p => new RealServiceProvider(serviceProvider.CreateScope().ServiceProvider));

            return dynamicProxyServices;
        }

        public IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            if (containerBuilder == null)
            {
                throw new ArgumentNullException(nameof(containerBuilder));
            }
            return CreateBuilder(containerBuilder).BuildServiceProvider().GetRequiredService<IServiceProvider>();
        }

        private static bool Validate(ServiceDescriptor descriptor, IAspectValidator aspectValidator, out Type implementationType)
        {
            implementationType = null;

            if (!aspectValidator.Validate(descriptor.ServiceType.GetTypeInfo()))
            {
                return false;
            }

            if (descriptor.ServiceType.GetTypeInfo().IsClass)
            {
                if (descriptor.ImplementationType == null)
                {
                    return false;
                }
                if (!descriptor.ImplementationType.GetTypeInfo().CanInherited())
                {
                    return false;
                }
            }

            implementationType = (Type)GetImplementationTypeAccessor.CreateMethodInvoker()(descriptor, new object[0]);

            if (implementationType == null)
            {
                return false;
            }

            return !implementationType.GetTypeInfo().IsDefined(typeof(DynamicallyAttribute));
        }
    }
}