using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 服务上下文接口,维护了需要管理的ServiceDefinition(类似：IServiceCollection)
    /// </summary>
    [NonAspect]
    public interface IServiceContext : IEnumerable<ServiceDefinition>
    {
        /// <summary>
        /// 单例生命周期的服务集合
        /// </summary>
        ILifetimeServiceContext Singletons { get; }

        /// <summary>
        /// 作用域生命周期的服务集合
        /// </summary>
        ILifetimeServiceContext Scopeds { get; }

        /// <summary>
        /// 瞬时生命周期的服务集合
        /// </summary>
        ILifetimeServiceContext Transients { get; }

        /// <summary>
        /// 配置
        /// </summary>
        IAspectConfiguration Configuration { get; }

        /// <summary>
        /// 服务描述对象的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 添加服务描述对象
        /// </summary>
        /// <param name="item">服务描述对象</param>
        void Add(ServiceDefinition item);

        /// <summary>
        /// 移除服务描述对象
        /// </summary>
        /// <param name="item">服务描述对象</param>
        /// <returns>移除是否成功</returns>
        bool Remove(ServiceDefinition item);

        /// <summary>
        /// 容器中是否包含有此类型的服务
        /// </summary>
        /// <param name="serviceType">服务</param>
        /// <returns>是否包含</returns>
        bool Contains(Type serviceType);
    }
}