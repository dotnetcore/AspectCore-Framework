using System;
using AspectCore.Abstractions;

namespace AspectCore.Core.DynamicProxy
{
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IProxyTypeGenerator _proxyTypeGenerator;
        private readonly IAspectActivatorFactory _aspectActivatorFactory;

        public ProxyGenerator(IProxyTypeGenerator proxyTypeGenerator, IAspectActivatorFactory aspectActivatorFactory)
        {
            _proxyTypeGenerator = proxyTypeGenerator ?? throw new ArgumentNullException(nameof(proxyTypeGenerator));
            _aspectActivatorFactory= aspectActivatorFactory ?? throw new ArgumentNullException(nameof(aspectActivatorFactory));
        }

        public object CreateClassProxy(Type serviceType, object implementationInstance)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }
            var proxyType = _proxyTypeGenerator.CreateClassProxyType(serviceType, implementationInstance.GetType());
            return Activator.CreateInstance(proxyType, _aspectActivatorFactory, implementationInstance);
        }

        public object CreateInterfaceProxy(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(serviceType);
            return Activator.CreateInstance(proxyType, _aspectActivatorFactory);
        }

        public object CreateInterfaceProxy(Type serviceType, object implementationInstance)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (implementationInstance == null)
            {
                return CreateInterfaceProxy(serviceType);
            }
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(serviceType, implementationInstance.GetType());
            return Activator.CreateInstance(proxyType, _aspectActivatorFactory, implementationInstance);
        }
    }
}