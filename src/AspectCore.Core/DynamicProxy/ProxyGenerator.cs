using System;
using AspectCore.DependencyInjection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 提供方法，生成代理
    /// </summary>
    [NonAspect]
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IProxyTypeGenerator _proxyTypeGenerator;
        private readonly IAspectActivatorFactory _aspectActivatorFactory;

        /// <summary>
        /// 提供方法，生成代理
        /// </summary>
        /// <param name="proxyTypeGenerator">代理类型生成器</param>
        /// <param name="aspectActivatorFactory">IAspectActivator对象的工厂</param>
        public ProxyGenerator(IProxyTypeGenerator proxyTypeGenerator, IAspectActivatorFactory aspectActivatorFactory)
        {
            _proxyTypeGenerator = proxyTypeGenerator ?? throw new ArgumentNullException(nameof(proxyTypeGenerator));
            _aspectActivatorFactory = aspectActivatorFactory ?? throw new ArgumentNullException(nameof(aspectActivatorFactory));
        }

        /// <summary>
        /// 代理类型生成器
        /// </summary>
        public IProxyTypeGenerator TypeGenerator => _proxyTypeGenerator;

        /// <summary>
        /// 生成类代理（继承方式实现代理）
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="implementationType">实现实例</param>
        /// <param name="args">构造器参数</param>
        /// <returns>代理对象</returns>
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
            //指定代理对象第一个构造参数为IAspectActivatorFactory
            proxyArgs[0] = _aspectActivatorFactory;
            for (var i = 0; i < args.Length; i++)
            {
                proxyArgs[i + 1] = args[i];
            }
            return Activator.CreateInstance(proxyType, proxyArgs);
        }

        /// <summary>
        /// 生成接口代理
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <returns>生成的代理对象</returns>
        public object CreateInterfaceProxy(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(serviceType);
            return Activator.CreateInstance(proxyType, _aspectActivatorFactory);
        }

        /// <summary>
        /// 生成接口代理
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="implementationInstance">实现实例</param>
        /// <returns>生成的代理对象</returns>
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