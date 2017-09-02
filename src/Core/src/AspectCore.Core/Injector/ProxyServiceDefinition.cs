using System;

namespace AspectCore.Injector
{
    internal class ProxyServiceDefinition : ServiceDefinition
    {
        public ServiceDefinition ServiceDefinition { get; }

        public ProxyServiceDefinition(ServiceDefinition serviceDefinition, Type proxyType) : base(serviceDefinition.ServiceType, serviceDefinition.Lifetime)
        {
            ProxyType = proxyType;
        }

        public Type ProxyType { get; }
    }
}