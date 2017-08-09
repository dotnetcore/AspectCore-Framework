using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using AspectCore.Abstractions;
using AspectCore.Extensions.IoC.Resolves;

namespace AspectCore.Extensions.IoC
{
    internal class ServiceResolver : IServiceResolver
    {
        private readonly ConcurrentDictionary<ServiceKey, IServiceFactory> _resolvedServices;
        internal readonly ServiceFactoryResolver _serviceFactoryResolver;

        internal ServiceResolver(IEnumerable<ServiceDefinition> services)
            : this(new ServiceFactoryResolver(services))
        {
        }

        internal ServiceResolver(ServiceFactoryResolver serviceFactoryResolver)
        {
            _resolvedServices = new ConcurrentDictionary<ServiceKey, IServiceFactory>();
            _serviceFactoryResolver = serviceFactoryResolver;
        }

        public void Dispose()
        {
            _serviceFactoryResolver.Dispose();
        }

        public object GetService(Type serviceType) => Resolve(serviceType, null);

        public object Resolve(Type serviceType, string key)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }
            var resolver = _resolvedServices.GetOrAdd(new ServiceKey(serviceType, key), _ => _serviceFactoryResolver.Resolve(_));
            if (resolver == null)
            {
                throw new InvalidOperationException($"No service keyd {key?.ToString() ?? "null"} for type '{serviceType}' has been registered.");
            }
            return resolver.Invoke(this);
        }
    }
}