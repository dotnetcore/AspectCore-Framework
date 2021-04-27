using System;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 扩展ILifetimeServiceContext接口，提供多种注册某种服务类型的方法
    /// </summary>
    public static class LifetimeServiceContextExtensions
    {
        /// <summary>
        /// 添加此种生命周期的服务
        /// </summary>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <param name="serviceType">要注册的服务类型</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddType(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType)
        {
            return AddType(lifetimeServiceContext, serviceType, serviceType);
        }

        /// <summary>
        /// 添加此种生命周期的服务
        /// </summary>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <param name="serviceType">要注册的服务类型</param>
        /// <param name="implementationType">要注册的服务实现类型</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddType(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType, Type implementationType)
        {
            if (lifetimeServiceContext == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContext));
            }
            lifetimeServiceContext.Add(new TypeServiceDefinition(serviceType, implementationType, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        /// <summary>
        /// 添加此种生命周期的服务
        /// </summary>
        /// <typeparam name="TService">要注册的服务类型</typeparam>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddType<TService>(this ILifetimeServiceContext lifetimeServiceContext)
        {
            return AddType(lifetimeServiceContext, typeof(TService), typeof(TService));
        }

        /// <summary>
        /// 添加此种生命周期的服务
        /// </summary>
        /// <typeparam name="TService">要注册的服务类型</typeparam>
        /// <typeparam name="TImplementation">服务的实现类型</typeparam>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddType<TService, TImplementation>(this ILifetimeServiceContext lifetimeServiceContext)
            where TImplementation : TService
        {
            if (lifetimeServiceContext == null)
            {
                throw new ArgumentNullException(nameof(lifetimeServiceContext));
            }
            lifetimeServiceContext.Add(new TypeServiceDefinition(typeof(TService), typeof(TImplementation), lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        /// <summary>
        /// 添加此种生命周期的服务
        /// </summary>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <param name="serviceType">要注册的服务类型</param>
        /// <param name="implementationInstance">要注册的服务实例</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddInstance(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType, object implementationInstance)
        {
            lifetimeServiceContext.Add(new InstanceServiceDefinition(serviceType, implementationInstance));
            return lifetimeServiceContext;
        }

        /// <summary>
        /// 添加此种生命周期的服务
        /// </summary>
        /// <typeparam name="TService">要注册的服务类型</typeparam>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <param name="implementationInstance">服务实例</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddInstance<TService>(this ILifetimeServiceContext lifetimeServiceContext, TService implementationInstance)
        {
            lifetimeServiceContext.Add(new InstanceServiceDefinition(typeof(TService), implementationInstance));
            return lifetimeServiceContext;
        }

        /// <summary>
        /// 添加此种生命周期的服务，实例由implementationDelegate委托所构建
        /// </summary>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <param name="serviceType">服务的类型</param>
        /// <param name="implementationDelegate">构建服务实例的委托</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddDelegate(this ILifetimeServiceContext lifetimeServiceContext, Type serviceType, Func<IServiceResolver, object> implementationDelegate)
        {
            lifetimeServiceContext.Add(new DelegateServiceDefinition(serviceType, implementationDelegate, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        /// <summary>
        /// 添加此种生命周期的服务，实例由implementationDelegate委托所构建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <typeparam name="TImplementation">实现类型</typeparam>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <param name="implementationDelegate">构建服务实例的委托</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddDelegate<TService, TImplementation>(this ILifetimeServiceContext lifetimeServiceContext, Func<IServiceResolver, TImplementation> implementationDelegate)
            where TService : class
            where TImplementation : class, TService
        {
            lifetimeServiceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }

        /// <summary>
        /// 添加此种生命周期的服务，服务由implementationDelegate委托所构建
        /// </summary>
        /// <typeparam name="TService">服务的类型</typeparam>
        /// <param name="lifetimeServiceContext">某一种生命周期的服务集合</param>
        /// <param name="implementationDelegate">构建服务的委托</param>
        /// <returns>某一种生命周期的服务集合</returns>
        public static ILifetimeServiceContext AddDelegate<TService>(this ILifetimeServiceContext lifetimeServiceContext, Func<IServiceResolver, TService> implementationDelegate)
           where TService : class
        {
            lifetimeServiceContext.Add(new DelegateServiceDefinition(typeof(TService), implementationDelegate, lifetimeServiceContext.Lifetime));
            return lifetimeServiceContext;
        }
    }
}