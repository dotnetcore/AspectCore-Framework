using System;
using System.Reflection;

namespace AspectCore.DependencyInjection
{
    public sealed class InstanceServiceDefinition : ServiceDefinition
    {
        public InstanceServiceDefinition(Type serviceType, object implementationInstance, object serviceKey = null) : base(serviceType, Lifetime.Singleton, serviceKey)
        {
            ImplementationInstance = implementationInstance ?? throw new ArgumentNullException(nameof(implementationInstance));
        }

        public object ImplementationInstance { get; }
    }
}