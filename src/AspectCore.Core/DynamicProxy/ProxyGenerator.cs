using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AspectCore.DependencyInjection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class ProxyGenerator : IProxyGenerator
    {
        private readonly IProxyTypeGenerator _proxyTypeGenerator;
        private readonly IAspectActivatorFactory _aspectActivatorFactory;
        private readonly IServiceProvider _serviceProvider;

        public ProxyGenerator(IProxyTypeGenerator proxyTypeGenerator, IAspectActivatorFactory aspectActivatorFactory, IServiceProvider serviceProvider)
        {
            _proxyTypeGenerator = proxyTypeGenerator ?? throw new ArgumentNullException(nameof(proxyTypeGenerator));
            _aspectActivatorFactory = aspectActivatorFactory ?? throw new ArgumentNullException(nameof(aspectActivatorFactory));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public IProxyTypeGenerator TypeGenerator => _proxyTypeGenerator;

        [RequiresDynamicCode("Creates proxy type instances via reflection. Use source-generated proxies for AOT compatibility.")]
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
            return CreateProxyInstance(proxyType, args);
        }

        [RequiresDynamicCode("Creates proxy type instances via reflection. Use source-generated proxies for AOT compatibility.")]
        public object CreateInterfaceProxy(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            var proxyType = _proxyTypeGenerator.CreateInterfaceProxyType(serviceType);
            return CreateInterfaceProxyInstance(proxyType, serviceType, implementationInstance: null);
        }

        [RequiresDynamicCode("Creates proxy type instances via reflection. Use source-generated proxies for AOT compatibility.")]
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
            return CreateInterfaceProxyInstance(proxyType, serviceType, implementationInstance);
        }

        [RequiresDynamicCode("Uses Activator.CreateInstance and ConstructorInfo.Invoke to create proxy instances.")]
        private object CreateProxyInstance(Type proxyType, object[] baseArgs)
        {
            // Try constructor with IServiceProvider first (SG-generated proxies)
            // Signature: (IAspectActivatorFactory, IServiceProvider, baseArgs...)
            var ctorParams = new Type[baseArgs.Length + 2];
            ctorParams[0] = typeof(IAspectActivatorFactory);
            ctorParams[1] = typeof(IServiceProvider);
            for (var i = 0; i < baseArgs.Length; i++)
                ctorParams[i + 2] = baseArgs[i]?.GetType();

            var ctor = proxyType.GetTypeInfo().GetConstructor(ctorParams);
            if (ctor != null)
            {
                var invokeArgs = new object[baseArgs.Length + 2];
                invokeArgs[0] = _aspectActivatorFactory;
                invokeArgs[1] = _serviceProvider;
                Array.Copy(baseArgs, 0, invokeArgs, 2, baseArgs.Length);
                return ctor.Invoke(invokeArgs);
            }

            // Fallback: legacy constructor (IAspectActivatorFactory, baseArgs...)
            var legacyParams = new Type[baseArgs.Length + 1];
            legacyParams[0] = typeof(IAspectActivatorFactory);
            for (var i = 0; i < baseArgs.Length; i++)
                legacyParams[i + 1] = baseArgs[i]?.GetType();

            var legacyCtor = proxyType.GetTypeInfo().GetConstructor(legacyParams);
            if (legacyCtor != null)
            {
                var invokeArgs = new object[baseArgs.Length + 1];
                invokeArgs[0] = _aspectActivatorFactory;
                Array.Copy(baseArgs, 0, invokeArgs, 1, baseArgs.Length);
                return legacyCtor.Invoke(invokeArgs);
            }

            // Last resort: Activator.CreateInstance with args
            var allArgs = new object[baseArgs.Length + 2];
            allArgs[0] = _aspectActivatorFactory;
            allArgs[1] = _serviceProvider;
            Array.Copy(baseArgs, 0, allArgs, 2, baseArgs.Length);
            return Activator.CreateInstance(proxyType, allArgs);
        }

        [RequiresDynamicCode("Uses Activator.CreateInstance and ConstructorInfo.Invoke to create proxy instances.")]
        private object CreateInterfaceProxyInstance(Type proxyType, Type serviceType, object implementationInstance)
        {
            // Try constructor with IServiceProvider first (SG-generated proxies)
            // Signature: (IAspectActivatorFactory, IServiceProvider, serviceType)
            if (implementationInstance != null)
            {
                var ctor3 = proxyType.GetTypeInfo().GetConstructor(
                    new Type[] { typeof(IAspectActivatorFactory), typeof(IServiceProvider), serviceType });
                if (ctor3 != null)
                {
                    return ctor3.Invoke(new object[] { _aspectActivatorFactory, _serviceProvider, implementationInstance });
                }
            }

            // Try: (IAspectActivatorFactory, IServiceProvider)
            var ctorSp = proxyType.GetTypeInfo().GetConstructor(
                new Type[] { typeof(IAspectActivatorFactory), typeof(IServiceProvider) });
            if (ctorSp != null)
            {
                return ctorSp.Invoke(new object[] { _aspectActivatorFactory, _serviceProvider });
            }

            // Fallback: legacy (IAspectActivatorFactory, serviceType)
            if (implementationInstance != null)
            {
                var ctor2 = proxyType.GetTypeInfo().GetConstructor(
                    new Type[] { typeof(IAspectActivatorFactory), serviceType });
                if (ctor2 != null)
                {
                    return ctor2.Invoke(new object[] { _aspectActivatorFactory, implementationInstance });
                }
            }

            // Fallback: legacy (IAspectActivatorFactory)
            var ctor1 = proxyType.GetTypeInfo().GetConstructor(
                new Type[] { typeof(IAspectActivatorFactory) });
            if (ctor1 != null)
            {
                return ctor1.Invoke(new object[] { _aspectActivatorFactory });
            }

            return Activator.CreateInstance(proxyType, _aspectActivatorFactory, _serviceProvider);
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