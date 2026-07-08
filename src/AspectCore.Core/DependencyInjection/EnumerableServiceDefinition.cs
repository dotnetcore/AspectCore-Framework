using System;
using System.Collections.Generic;

namespace AspectCore.DependencyInjection
{
    internal class EnumerableServiceDefinition : ServiceDefinition
    {
        public IEnumerable<ServiceDefinition> ServiceDefinitions { get; }

        public Type ElementType { get; }

        public EnumerableServiceDefinition(Type serviceType, Type elementType, IEnumerable<ServiceDefinition> serviceDefinitions) : base(serviceType, Lifetime.Transient)
        {
            ElementType = elementType;
            ServiceDefinitions = serviceDefinitions;
        }
    }

    internal class ManyEnumerableServiceDefinition : EnumerableServiceDefinition
    {
        public ManyEnumerableServiceDefinition(Type serviceType, Type elementType, IEnumerable<ServiceDefinition> serviceDefinitions) : base(serviceType, elementType, serviceDefinitions)
        {
        }
    }
}