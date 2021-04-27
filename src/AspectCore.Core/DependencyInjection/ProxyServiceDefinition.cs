using System;
using System.Reflection;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 代理服务描述
    /// </summary>
    internal class ProxyServiceDefinition : ServiceDefinition
    {
        /// <summary>
        /// 当实现代理的方式为接口代理方式,此属性表示针对的接口的服务描述
        /// </summary>
        public ServiceDefinition ServiceDefinition { get; }

        /// <summary>
        /// 当实现代理的方式为子类代理方式,此属性表示被代理类的服务描述
        /// </summary>
        public TypeServiceDefinition ClassProxyServiceDefinition { get; }

        /// <summary>
        /// 构造代理服务描述
        /// </summary>
        /// <param name="serviceDefinition">1：接口代理则为接口的服务描述,2:类代理则为被代理对象的服务描述</param>
        /// <param name="proxyType">代理的类型</param>
        public ProxyServiceDefinition(ServiceDefinition serviceDefinition, Type proxyType) : base(serviceDefinition.ServiceType, serviceDefinition.Lifetime)
        {
            ProxyType = proxyType;
            ServiceDefinition = serviceDefinition;
            if (serviceDefinition.ServiceType.GetTypeInfo().IsClass)
            {
                ClassProxyServiceDefinition = new TypeServiceDefinition(serviceDefinition.ServiceType, ProxyType, serviceDefinition.Lifetime);
            }
        }

        /// <summary>
        /// 代理的类型
        /// </summary>
        public Type ProxyType { get; }
    }
}