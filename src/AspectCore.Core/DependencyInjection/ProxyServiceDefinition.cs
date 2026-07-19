using System;
using System.Reflection;

namespace AspectCore.DependencyInjection
{
    internal class ProxyServiceDefinition : ServiceDefinition
    {
        public ServiceDefinition ServiceDefinition { get; }
        public TypeServiceDefinition ClassProxyServiceDefinition { get; }

        public ProxyServiceDefinition(ServiceDefinition serviceDefinition, Type proxyType) : base(serviceDefinition.ServiceType, serviceDefinition.Lifetime, serviceDefinition.ServiceKey)
        {
            ProxyType = proxyType;
            ServiceDefinition = serviceDefinition;
            if (serviceDefinition.ServiceType.GetTypeInfo().IsClass)
            {
                ClassProxyServiceDefinition = new TypeServiceDefinition(serviceDefinition.ServiceType, ProxyType, serviceDefinition.Lifetime, serviceDefinition.ServiceKey);
            }
        }

        public Type ProxyType { get; }
    }
}