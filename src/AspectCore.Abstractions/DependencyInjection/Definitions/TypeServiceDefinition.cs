using System;
using System.Reflection;

namespace AspectCore.DependencyInjection
{
    public sealed class TypeServiceDefinition : ServiceDefinition
    {
        public TypeServiceDefinition(Type serviceType, Type implementationType, Lifetime lifetime, object serviceKey = null) : base(serviceType, lifetime, serviceKey)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }

        public Type ImplementationType { get; }
    }
}