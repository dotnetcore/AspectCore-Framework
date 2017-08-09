using System;
using System.Collections.Generic;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class EnumerableServiceFactory : IServiceFactory
    {
        public ServiceKey ServiceKey { get; }

        public ServiceDefinition ServiceDefinition { get; }

        private readonly IServiceFactory[] _servicesFactories;

        public EnumerableServiceFactory(Type itemType, IServiceFactory[] servicesFactories)
        {
            ServiceDefinition = new TypeServiceDefinition(typeof(IEnumerable<>).MakeGenericType(itemType), itemType.MakeArrayType(), Lifetime.Transient, null);
            ServiceKey = new ServiceKey(ServiceDefinition.ServiceType, null);
            _servicesFactories = servicesFactories;
        }

        public object Invoke(IServiceResolver serviceResolver)
        {
            var length = _servicesFactories.Length;
            var results = new object[length];
            for (var i = 0; i < length; i++)
            {
                results[i] = _servicesFactories[i].Invoke(serviceResolver);
            }
            return results;
        }
    }
}