using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    public static class ServiceContextExtensions
    {
        public static IServiceContext AddType(this IServiceContext serviceContext, Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContext, serviceType, serviceType, lifetime);
        }

        public static IServiceContext AddType(this IServiceContext serviceContext, Type serviceType, Type implementationType, Lifetime lifetime = Lifetime.Transient)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            serviceContext.Add(new TypeServiceDefinition(serviceType, implementationType, lifetime));
            return serviceContext;
        }

        public static IServiceContext AddType<TService>(this IServiceContext serviceContext, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContext, typeof(TService), lifetime);
        }

        public static IServiceContext AddType<TService, TImplementation>(this IServiceContext serviceContext, Lifetime lifetime = Lifetime.Transient)
            where TImplementation : TService
        {
            return AddType(serviceContext, typeof(TService), typeof(TImplementation), lifetime);
        }

        public static IServiceContext AddInstance(this IServiceContext serviceContext, Type serviceType, object implementationInstance)
        {
            serviceContext.Add(new InstanceServiceDefinition(serviceType, implementationInstance));
            return serviceContext;
        }

        public static IServiceContext AddInstance<TService>(this IServiceContext serviceContext, TService implementationInstance)
        {
            serviceContext.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance));
            return serviceContext;
        }

        public static IServiceContext AddDelegate(this IServiceContext serviceContext, Type serviceType, Func<IServiceResolver, object> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
        {
            serviceContext.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetime));
            return serviceContext;
        }

        public static IServiceContext AddDelegate<TService, TImplementation>(this IServiceContext serviceContext, Func<IServiceResolver, TImplementation> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            serviceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetime));
            return serviceContext;
        }

        public static IServiceContext AddDelegate<TService>(this IServiceContext serviceContext, Func<IServiceResolver, TService> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
           where TService : class
        {
            serviceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetime));
            return serviceContext;
        }

        public static IServiceContext RemoveAll<TService>(this IServiceContext serviceContext) where TService : class
        {
            return RemoveAll(serviceContext, typeof(TService));
        }

        public static IServiceContext RemoveAll(this IServiceContext serviceContext, Type serviceType)
        {
            var serviceDefinitions = new List<ServiceDefinition>();
            foreach (var serviceDefinition in serviceContext)
            {
                if (serviceDefinition.ServiceType == serviceType)
                {
                    serviceDefinitions.Add(serviceDefinition);
                }
            }

            serviceDefinitions.ForEach(t => serviceContext.Remove(t));
            return serviceContext;
        }

        /// <summary>
        /// 配置 AOP 后端引擎（DynamicProxy / SourceGenerator / Auto）。
        /// 
        /// ServiceContext 下该配置会被运行时解析并用于选择 IProxyTypeGenerator 的实现。
        /// </summary>
        public static IServiceContext ConfigureDynamicProxyEngine(this IServiceContext serviceContext, Action<ProxyEngineOptions> configure)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }

            var existing = serviceContext
                .OfType<InstanceServiceDefinition>()
                .LastOrDefault(x => x.ServiceType == typeof(ProxyEngineOptions));

            var options = (ProxyEngineOptions)existing?.ImplementationInstance ?? new ProxyEngineOptions();
            configure?.Invoke(options);

            if (existing != null)
            {
                serviceContext.Remove(existing);
            }

            serviceContext.AddInstance(options);
            return serviceContext;
        }

        /// <summary>
        /// 手动注册生成的 registry（用于 AOT/trim 场景避免 assembly 扫描不可用）。
        /// </summary>
        public static IServiceContext AddSourceGeneratedProxyRegistry(this IServiceContext serviceContext, ISourceGeneratedProxyRegistry registry)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            if (registry == null)
            {
                throw new ArgumentNullException(nameof(registry));
            }
            serviceContext.AddInstance<ISourceGeneratedProxyRegistry>(registry);
            return serviceContext;
        }

    }
}
