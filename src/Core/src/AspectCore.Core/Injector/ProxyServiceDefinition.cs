using System;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injector
{
    internal class ProxyServiceDefinition : ServiceDefinition
    {
        public ServiceDefinition ServiceDefinition { get; }

        public ProxyServiceDefinition(ServiceDefinition serviceDefinition,Type proxyType) : base(serviceDefinition.ServiceType, serviceDefinition.Lifetime)
        {
            ProxyType = proxyType;
        }

        public ProxyServiceDefinition(ServiceDefinition serviceDefinition, Func<IServiceResolver, object> proxyFactory) : base(serviceDefinition.ServiceType, serviceDefinition.Lifetime)
        {
            ProxyFactory = proxyFactory;
        }

        public Type ProxyType { get; }

        public Func<IServiceResolver, object> ProxyFactory { get; }
    }
}
