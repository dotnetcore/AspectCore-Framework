using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injector
{
    internal class ProxyServiceDefinition : ServiceDefinition
    {
        public ServiceDefinition ServiceDefinition { get; }

        public ProxyServiceDefinition(ServiceDefinition serviceDefinition) : base(serviceDefinition.ServiceType, serviceDefinition.Lifetime)
        {
        }

        public Type ProxyType { get; }

        public Func<IServiceResolver, object> ProxyFactory { get; }
    }
}
