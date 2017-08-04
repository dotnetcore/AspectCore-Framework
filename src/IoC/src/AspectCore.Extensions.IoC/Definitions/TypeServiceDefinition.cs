using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Definitions
{
    public sealed class TypeServiceDefinition : ServiceDefinition
    {
        public TypeServiceDefinition(Type serviceType, Type implementationType, Lifetime lifetime, object key) : base(serviceType, lifetime, key)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            if (!serviceType.GetTypeInfo().IsAssignableFrom(implementationType))
            {
                throw new ArgumentException($"{implementationType} is not a subclass or implementation type of {serviceType}.", nameof(implementationType));
            }
        }

        public Type ImplementationType { get; }
    }
}