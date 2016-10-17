using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Generators;
using System;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.DependencyInjection;
using AspectCore.Lite.Internal;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public class ProxyActivator : IProxyActivator
    {
        private readonly IServiceProvider serviceProvider;

        public ProxyActivator()
            : this(ServiceCollectionUtilities.CreateAspectLiteServices().BuildServiceProvider())
        {
        }

        public ProxyActivator(IServiceProvider serviceProvider)
        {
            ExceptionUtilities.ThrowArgumentNull(serviceProvider , nameof(serviceProvider));
            this.serviceProvider = serviceProvider;
        }

        public object CreateClassProxy(Type serviceType , object instance , params Type[] interfaceTypes)
        {
            ExceptionUtilities.ThrowArgumentNull(serviceType , nameof(serviceType));
            ExceptionUtilities.ThrowArgumentNull(instance , nameof(instance));
            ExceptionUtilities.ThrowArgument(() => !serviceType.GetTypeInfo().IsAssignableFrom(instance.GetType()) , $"Can not assign an instance of the {instance.GetType()} to the {serviceType}.");

            var proxyGenerator = new ClassProxyGenerator(serviceProvider , serviceType , interfaceTypes);
            var proxyType = proxyGenerator.GenerateProxyType();
            return ActivatorUtilities.CreateInstance(serviceProvider , proxyType , new object[] { serviceProvider , instance });
        }

        public object CreateInterfaceProxy(Type serviceType , object instance , params Type[] interfaceTypes)
        {
            ExceptionUtilities.ThrowArgumentNull(serviceType , nameof(serviceType));
            ExceptionUtilities.ThrowArgumentNull(instance , nameof(instance));
            ExceptionUtilities.ThrowArgument(() => !serviceType.GetTypeInfo().IsAssignableFrom(instance.GetType()) , $"Can not assign an instance of the {instance.GetType()} to the {serviceType}.");

            var proxyGenerator = new InterfaceProxyGenerator(serviceProvider , serviceType , interfaceTypes);
            var proxyType = proxyGenerator.GenerateProxyType();
            return ActivatorUtilities.CreateInstance(serviceProvider , proxyType , new object[] { serviceProvider , instance });
        }
    }
}
