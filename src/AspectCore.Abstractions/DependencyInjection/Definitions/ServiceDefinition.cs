using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public abstract class ServiceDefinition
    {
        public Type ServiceType { get; }

        public Lifetime Lifetime { get; }

        public ServiceDefinition(Type serviceType, Lifetime lifetime)
        {
            Lifetime = lifetime;
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
        }
    }
}