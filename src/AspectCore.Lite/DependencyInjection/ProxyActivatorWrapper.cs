using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Activators;
using System;

namespace AspectCore.Lite.DependencyInjection
{
    internal class ProxyActivatorWrapper : IProxyActivator
    {
        private readonly IServiceProviderWrapper serviceProvider;
        private readonly IProxyActivator proxyActivator;

        public ProxyActivatorWrapper(IServiceProviderWrapper serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.proxyActivator = new ProxyActivator(serviceProvider);
        }

        public object CreateClassProxy(Type serviceType , object instance , params Type[] interfaceTypes)
        {
            return proxyActivator.CreateClassProxy(serviceType , instance , interfaceTypes);
        }

        public object CreateInterfaceProxy(Type serviceType , object instance , params Type[] interfaceTypes)
        {
            return proxyActivator.CreateInterfaceProxy(serviceType , instance , interfaceTypes);
        }
    }
}
