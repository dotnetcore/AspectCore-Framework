using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using AspectCore.Abstractions;
using System.Linq;

namespace AspectCore.Extensions.IoC.Resolves
{
    internal sealed class ServiceFactoryResolver : IDisposable
    {
        private readonly object _root;
        private readonly bool _enableProxy;
        private readonly Dictionary<Type, ServiceDefinition[]> _serviceDefinitions;

        public ServiceFactoryResolver(IEnumerable<ServiceDefinition> services, bool enableProxy)
        {
            _enableProxy = enableProxy;
            _serviceDefinitions = new Dictionary<Type, ServiceDefinition[]>();
            //添加ServiceContainerServiceFactory
        }

        private void Populate(IEnumerable<ServiceDefinition> services)
        {
            foreach (var service in services.GroupBy(x => x.ServiceType))
            {
                _serviceDefinitions.Add(service.Key, service.ToArray());
            }

        }

        public ServiceFactoryResolver(ServiceFactoryResolver root, bool enableProxy)
        {
            _root = root;
            _enableProxy = enableProxy;
            _serviceDefinitions = root._serviceDefinitions;
        }

        public IServiceFactory Resolve(ServiceKey key)
        {
            if (_enableProxy)
            {

            }
            else
            {

            }
            return null;
        }

        public ServiceFactoryResolver CreateScope()
        {
            return new ServiceFactoryResolver(this, _enableProxy);
        }

        public void Dispose()
        {
            if (_root != null)
            {

            }
        }
    }
}