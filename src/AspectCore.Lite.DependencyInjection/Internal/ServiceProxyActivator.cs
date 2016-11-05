using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Activators;
using System;

namespace AspectCore.Lite.DependencyInjection.Internal
{
    internal class ServiceProxyActivator : IProxyActivator
    {
        private readonly IProxyServiceProvider serviceProvider;
        private readonly IProxyActivator proxyActivator;

        public ServiceProxyActivator(IProxyServiceProvider serviceProvider)
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
