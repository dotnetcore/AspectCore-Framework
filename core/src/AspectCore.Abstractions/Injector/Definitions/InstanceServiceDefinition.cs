using System;
using System.Reflection;

namespace AspectCore.Injector
{
    public sealed class InstanceServiceDefinition : ServiceDefinition
    {
        public InstanceServiceDefinition(Type serviceType, object implementationInstance) : base(serviceType, Lifetime.Singleton)
        {
            ImplementationInstance = implementationInstance ?? throw new ArgumentNullException(nameof(implementationInstance));
        }

        public object ImplementationInstance { get; }
    }
}