using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Abstractions.Activators;
using System;

namespace AspectCore.Lite.Internal
{
    internal class ServiceProxyActivator : IProxyActivator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IProxyActivator proxyActivator;

        public ServiceProxyActivator(IServiceProvider serviceProvider)
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
