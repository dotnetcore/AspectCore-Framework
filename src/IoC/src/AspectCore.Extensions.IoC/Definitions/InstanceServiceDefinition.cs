using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Definitions
{
    public sealed class InstanceServiceDefinition : ServiceDefinition
    {
        public InstanceServiceDefinition(Type serviceType, object implementationInstance, Lifetime lifetime, object key) : base(serviceType, lifetime, key)
        {
            ImplementationInstance = implementationInstance ?? throw new ArgumentNullException(nameof(implementationInstance));
        }

        public object ImplementationInstance { get; }
    }
}
