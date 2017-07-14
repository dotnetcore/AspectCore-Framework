using System;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AspectCore.Extensions.DependencyInjection
{
    public class AspectCoreServiceProviderFactory : IServiceProviderFactory<IServiceCollection>
    {

        private readonly static MethodInfo GetImplementationType = typeof(ServiceDescriptor).GetTypeInfo().GetMethod("GetImplementationType", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly static MethodReflector GetImplementationTypeAccessor = new MethodReflector(GetImplementationType);

        public IServiceCollection CreateBuilder(IServiceCollection serviceCollection)
        {
            if (serviceCollection == null)
            {
                throw new ArgumentNullException(nameof(serviceCollection));
            }
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var aspectValidator = serviceProvider.GetRequiredService<IAspectValidatorBuilder>().Build();

            var dynamicProxyServices = new ServiceCollection();

            var generator = serviceProvider.GetRequiredService<IProxyGenerator>();

            foreach (var descriptor in serviceCollection)
            {
                Type proxyType, implementationType;
                if (!Validate(descriptor, aspectValidator, out implementationType))
                {
                    dynamicProxyServices.Add(descriptor);
                    continue;
                }

                if (descriptor.ServiceType.GetTypeInfo().IsInterface)
                {
                    if (serviceCollection.Count(x => x.ServiceType == descriptor.ServiceType) > 1)
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
            return CreateBuilder(containerBuilder).BuildServiceProvider();
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

            if (implementationType.GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)))
            {
                return false;
            }

            return true;
        }
    }
}