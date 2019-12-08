using System;
using System.Reflection;

namespace AspectCore.DependencyInjection
{
    public sealed class TypeServiceDefinition : ServiceDefinition
    {
        public TypeServiceDefinition(Type serviceType, Type implementationType, Lifetime lifetime) : base(serviceType, lifetime)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
        }

        public Type ImplementationType { get; }
    }
}