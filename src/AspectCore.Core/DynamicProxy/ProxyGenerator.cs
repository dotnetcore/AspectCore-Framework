using System;
using AspectCore.DependencyInjection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IProxyTypeGenerator _proxyTypeGenerator;
        private readonly IAspectActivatorFactory _aspectActivatorFactory;

        public ProxyGenerator(IProxyTypeGenerator proxyTypeGenerator, IAspectActivatorFactory aspectActivatorFactory)
        {
            _proxyTypeGenerator = proxyTypeGenerator ?? throw new ArgumentNullException(nameof(proxyTypeGenerator));
            _aspectActivatorFactory = aspectActivatorFactory ?? throw new ArgumentNullException(nameof(aspectActivatorFactory));
        }

        public IProxyTypeGenerator TypeGenerator => _proxyTypeGenerator;

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

        public void Dispose()
        {
        }
    }

    internal sealed class DisposedProxyGenerator : IProxyGenerator
    {
        private readonly IServiceResolver _serviceResolver;
        private readonly IProxyGenerator _proxyGenerator;

        public DisposedProxyGenerator(IServiceResolver serviceResolver)
        {
            _serviceResolver = serviceResolver;
            _proxyGenerator = serviceResolver.ResolveRequired<IProxyGenerator>();
        }

        public IProxyTypeGenerator TypeGenerator => _proxyGenerator.TypeGenerator;

        public object CreateClassProxy(Type serviceType, Type implementationType, object[] args)
        {
            return _proxyGenerator.CreateClassProxy(serviceType, implementationType, args);
        }

        public object CreateInterfaceProxy(Type serviceType)
        {
            return _proxyGenerator.CreateInterfaceProxy(serviceType);
        }

        public object CreateInterfaceProxy(Type serviceType, object implementationInstance)
        {
            return _proxyGenerator.CreateInterfaceProxy(serviceType, implementationInstance);
        }

        public void Dispose()
        {
            _serviceResolver.Dispose();
        }
    }
}