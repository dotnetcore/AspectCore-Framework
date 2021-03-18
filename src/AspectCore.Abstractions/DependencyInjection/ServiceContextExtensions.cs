using System;
using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 扩展IServiceContext,提供注册服务的方法
    /// </summary>
    public static class ServiceContextExtensions
    {
        /// <summary>
        /// 注册服务到容器
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="serviceType">实现类型</param>
        /// <param name="lifetime">生命周期</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddType(this IServiceContext serviceContext, Type serviceType, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContext, serviceType, serviceType, lifetime);
        }

        /// <summary>
        /// 注册服务到容器
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationType">实现类型</param>
        /// <param name="lifetime">生命周期</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddType(this IServiceContext serviceContext, Type serviceType, Type implementationType, Lifetime lifetime = Lifetime.Transient)
        {
            if (serviceContext == null)
            {
                throw new ArgumentNullException(nameof(serviceContext));
            }
            serviceContext.Add(new TypeServiceDefinition(serviceType, implementationType, lifetime));
            return serviceContext;
        }

        /// <summary>
        /// 注册服务到容器
        /// </summary>
        /// <typeparam name="TService">暴露的服务类型</typeparam>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="lifetime">生命周期</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddType<TService>(this IServiceContext serviceContext, Lifetime lifetime = Lifetime.Transient)
        {
            return AddType(serviceContext, typeof(TService), lifetime);
        }

        /// <summary>
        /// 注册服务到容器
        /// </summary>
        /// <typeparam name="TService">暴露的服务类型</typeparam>
        /// <typeparam name="TImplementation">实现类型</typeparam>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="lifetime">生命周期</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddType<TService, TImplementation>(this IServiceContext serviceContext, Lifetime lifetime = Lifetime.Transient)
            where TImplementation : TService
        {
            return AddType(serviceContext, typeof(TService), typeof(TImplementation), lifetime);
        }

        /// <summary>
        /// 注册服务实例到容器
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationInstance">实现类型的实例</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddInstance(this IServiceContext serviceContext, Type serviceType, object implementationInstance)
        {
            serviceContext.Add(new InstanceServiceDefinition(serviceType, implementationInstance));
            return serviceContext;
        }

        /// <summary>
        /// 注册服务实例到容器
        /// </summary>
        /// <typeparam name="TService">暴露的服务类型</typeparam>
        /// <param name="serviceContext">暴露的服务类型</param>
        /// <param name="implementationInstance">实现类型的实例</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddInstance<TService>(this IServiceContext serviceContext, TService implementationInstance)
        {
            serviceContext.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance));
            return serviceContext;
        }

        /// <summary>
        /// 添加委托类型的服务描述对象到容器
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="serviceType">暴露的服务类型</param>
        /// <param name="implementationDelegate">要添加的委托</param>
        /// <param name="lifetime">生命周期</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddDelegate(this IServiceContext serviceContext, Type serviceType, Func<IServiceResolver, object> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
        {
            serviceContext.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetime));
            return serviceContext;
        }

        /// <summary>
        /// 添加委托类型的服务描述对象到容器
        /// </summary>
        /// <typeparam name="TService">暴露的服务类型</typeparam>
        /// <typeparam name="TImplementation">实现类型</typeparam>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="implementationDelegate">要添加的委托</param>
        /// <param name="lifetime">生命周期</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddDelegate<TService, TImplementation>(this IServiceContext serviceContext, Func<IServiceResolver, TImplementation> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
            where TService : class
            where TImplementation : class, TService
        {
            serviceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetime));
            return serviceContext;
        }

        /// <summary>
        /// 添加委托类型的服务描述对象到容器
        /// </summary>
        /// <typeparam name="TService">委托的返回类型</typeparam>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="implementationDelegate">Func<IServiceResolver, TService>委托</param>
        /// <param name="lifetime">生命周期</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext AddDelegate<TService>(this IServiceContext serviceContext, Func<IServiceResolver, TService> implementationDelegate, Lifetime lifetime = Lifetime.Transient)
           where TService : class
        {
            serviceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetime));
            return serviceContext;
        }

        /// <summary>
        /// 从容器中移除所有TService类型的服务
        /// </summary>
        /// <typeparam name="TService">服务类型</typeparam>
        /// <param name="serviceContext">服务上下文</param>
        /// <returns>服务上下文</returns>
        public static IServiceContext RemoveAll<TService>(this IServiceContext serviceContext) where TService : class
        {
            return RemoveAll(serviceContext, typeof(TService));
        }

        /// <summary>
        /// 从容器中移除所有serviceType类型的服务
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        /// <param name="serviceType">服务类型</param>
        /// <returns>服务上下文</returns>
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

    }
}
