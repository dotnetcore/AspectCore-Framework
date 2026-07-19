using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public abstract class ServiceDefinition
    {
        public Type ServiceType { get; }

        public Lifetime Lifetime { get; }

        /// <summary>
        /// Gets the service key associated with a keyed service registration, or <c>null</c> for a non-keyed service.
        /// </summary>
        public object ServiceKey { get; }

        public ServiceDefinition(Type serviceType, Lifetime lifetime, object serviceKey = null)
        {
            Lifetime = lifetime;
            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            ServiceKey = serviceKey;
        }
    }
}
