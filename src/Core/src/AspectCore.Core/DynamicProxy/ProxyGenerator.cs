using System;

namespace AspectCore.DynamicProxy
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

        public object CreateClassProxy(Type serviceType, Type implementationType, object[] args)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }
            if (args == null)
            {
                throw new ArgumentNullException(nameof(args));
            }
            var proxyType = _proxyTypeGenerator.CreateClassProxyType(serviceType, implementationType);
            var proxyArgs = new object[args.Length + 1];
            proxyArgs[0] = _aspectActivatorFactory;
            for (var i = 0; i < args.Length; i++)
            {
                proxyArgs[i + 1] = args[i];
            }
            return Activator.CreateInstance(proxyType, proxyArgs);
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