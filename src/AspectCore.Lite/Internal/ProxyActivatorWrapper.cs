using AspectCore.Lite.Abstractions;
using AspectCore.Lite.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
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
