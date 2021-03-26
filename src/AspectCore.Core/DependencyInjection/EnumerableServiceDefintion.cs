using System;
using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 一组相同类型的服务描述对象
    /// </summary>
    internal class EnumerableServiceDefintion : ServiceDefinition
    {
        public IEnumerable<ServiceDefinition> ServiceDefinitions { get; }

        /// <summary>
        /// 这组服务描述对象中每个元素的类型
        /// </summary>
        public Type ElementType { get; }

        /// <summary>
        /// 一组相同类型的服务描述对象
        /// </summary>
        /// <param name="serviceType">服务类型</param>
        /// <param name="elementType">这组服务描述对象中每个元素的类型</param>
        /// <param name="serviceDefinitions">服务描述集合</param>
        public EnumerableServiceDefintion(Type serviceType, Type elementType, IEnumerable<ServiceDefinition> serviceDefinitions) : base(serviceType, Lifetime.Transient)
        {
            ElementType = elementType;
            ServiceDefinitions = serviceDefinitions;
        }
    }

    internal class ManyEnumerableServiceDefintion : EnumerableServiceDefintion
    {
        public ManyEnumerableServiceDefintion(Type serviceType, Type elementType, IEnumerable<ServiceDefinition> serviceDefinitions) : base(serviceType, elementType, serviceDefinitions)
        {
        }
    }
}