using System;
using System.Collections.Generic;

namespace AspectCore.Injector
{
    internal class EnumerableServiceDefintion : ServiceDefinition
    {
        public IEnumerable<ServiceDefinition> ServiceDefinitions { get; }

        public Type ElementType { get; }

        public EnumerableServiceDefintion(Type serviceType, Type elementType, IEnumerable<ServiceDefinition> serviceDefinitions) : base(serviceType, Lifetime.Transient)
        {
            ElementType = elementType;
            ServiceDefinitions = serviceDefinitions;
        }
    }
}