using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public sealed class InstanceServiceDefinition : ServiceDefinition
    {
        public InstanceServiceDefinition(Type serviceType, object implementationInstance, object key) : base(serviceType, Lifetime.Singleton, key)
        {
            ImplementationInstance = implementationInstance ?? throw new ArgumentNullException(nameof(implementationInstance));
            if (!serviceType.GetTypeInfo().IsAssignableFrom(implementationInstance.GetType()))
            {
                throw new ArgumentException($"Instance is not a subclass or implementation of type '{serviceType}'.", nameof(implementationInstance));
            }
        }

        public object ImplementationInstance { get; }
    }
}