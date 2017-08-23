using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    internal class ServiceResolver : IServiceResolver
    {
        private readonly ConcurrentDictionary<Type, object> resolvedScopedServcies;
        private readonly ConcurrentDictionary<Type, object> resolvedSingletonServcies;
        private readonly IEnumerable<ServiceDefinition> _initialServiceDefinitions;
        private readonly Dictionary<Type, LinkedList<ServiceDefinition>> _linkedServiceDefinitions;


        public ServiceResolver(IEnumerable<ServiceDefinition> serviceDefinitions)
        {
            _initialServiceDefinitions = serviceDefinitions;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            if (_linkedServiceDefinitions.TryGetValue(serviceType, out var value))
            {
                if(value.Last.Value.Lifetime==
            }
        }
    }
}