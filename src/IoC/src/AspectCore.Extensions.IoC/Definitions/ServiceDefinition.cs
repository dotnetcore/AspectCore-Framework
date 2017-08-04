using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Definitions
{
    public abstract class ServiceDefinition : IServiceDefinition
    {
        public object Key { get; }

        public Type ServiceType { get; }

        public Lifetime Lifetime { get; }

        public ServiceDefinition(Type serviceType, Lifetime lifetime, object key)
        {
            Key = key;
            Lifetime = lifetime;
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }
    }
}
