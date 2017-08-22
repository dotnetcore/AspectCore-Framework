using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public sealed class TypeServiceDefinition : ServiceDefinition
    {
        public TypeServiceDefinition(Type serviceType, Type implementationType, Lifetime lifetime, string key) : base(serviceType, lifetime, key)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            if (!serviceType.GetTypeInfo().IsAssignableFrom(implementationType))
            {
                throw new ArgumentException($"Type '{implementationType}' is not a subclass or implementation type of type '{serviceType}'.", nameof(implementationType));
            }
        }

        public Type ImplementationType { get; }
    }
}