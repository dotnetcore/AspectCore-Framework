using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 具有同一种生命周期的服务描述(ServiceDefinition)集合
    /// </summary>
    public sealed class LifetimeServiceContext : ILifetimeServiceContext
    {
        private readonly ICollection<ServiceDefinition> _internalCollection;

        /// <summary>
        /// 生命周期
        /// </summary>
        public Lifetime Lifetime { get; }

        /// <summary>
        /// 具有同一种生命周期的服务描述(ServiceDefinition)集合
        /// </summary>
        /// <param name="collection">具有同一种生命周期的服务描述对象</param>
        /// <param name="lifetime">生命周期</param>
        public LifetimeServiceContext(ICollection<ServiceDefinition> collection, Lifetime lifetime)
        {
            _internalCollection = collection;
            Lifetime = lifetime;
        }

        /// <summary>
        /// 服务描述的数量
        /// </summary>
        public int Count => _internalCollection.Count(x => x.Lifetime == Lifetime);

        /// <summary>
        /// 添加服务描述对象
        /// </summary>
        /// <param name="item">服务描述对象</param>
        public void Add(ServiceDefinition item)
        {
            if (item.Lifetime == Lifetime)
            {
                _internalCollection.Add(item);
            }
        }

        /// <summary>
        /// 判断此生命周期的服务描述集合中是否包含serviceType类型
        /// </summary>
        /// <param name="serviceType">待判断的服务类型</param>
        /// <returns>是否包含</returns>
        public bool Contains(Type serviceType) => _internalCollection.Any(x => x.ServiceType == serviceType && x.Lifetime == Lifetime);

        public IEnumerator<ServiceDefinition> GetEnumerator() => _internalCollection.Where(x => x.Lifetime == Lifetime).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}