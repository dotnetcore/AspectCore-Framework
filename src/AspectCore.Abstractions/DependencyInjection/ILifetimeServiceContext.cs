using System;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 服务描述(ServiceDefinition)集合，集合中的ServiceDefinition具有同一种生命周期
    /// </summary>
    [NonAspect]
    public interface ILifetimeServiceContext : IEnumerable<ServiceDefinition>
    {
        /// <summary>
        /// 生命周期
        /// </summary>
        Lifetime Lifetime { get; }

        /// <summary>
        /// 内部维护的ServiceDefinition的数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 添加服务描述对象
        /// </summary>
        /// <param name="item">服务描述对象</param>
        void Add(ServiceDefinition item);

        /// <summary>
        /// 判断此生命周期的服务上下文中是否包含serviceType类型
        /// </summary>
        /// <param name="serviceType">待判断的服务类型</param>
        /// <returns>是否包含</returns>
        bool Contains(Type serviceType);
    }
}