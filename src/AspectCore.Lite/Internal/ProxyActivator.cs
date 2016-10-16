using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Generators;
using System;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.DependencyInjection;

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
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            this.serviceProvider = serviceProvider;
        }

        public object CreateClassProxy(Type serviceType , object instance , Type[] interfaceTypes)
        {
            var proxyGenerator = new ClassProxyGenerator(serviceProvider , serviceType , interfaceTypes);
            var proxyType = proxyGenerator.GenerateProxyType();
            return ActivatorUtilities.CreateInstance(serviceProvider , proxyType , new object[] { serviceProvider , instance });
        }

        public object CreateInterfaceProxy(Type serviceType , object instance , Type[] interfaceTypes)
        {
            var proxyGenerator = new InterfaceProxyGenerator(serviceProvider , serviceType , interfaceTypes);
            var proxyType = proxyGenerator.GenerateProxyType();
            return ActivatorUtilities.CreateInstance(serviceProvider , proxyType , new object[] { serviceProvider , instance });
        }
    }
}
