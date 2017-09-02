using System;
using System.Collections.Concurrent;
using AspectCore.Abstractions;

namespace AspectCore.Core.Injector
{
    internal class ServiceResolver : IServiceResolver
    {
        private readonly ConcurrentDictionary<ServiceDefinition, object> _resolvedScopedServcies;
        private readonly ConcurrentDictionary<ServiceDefinition, object> _resolvedSingletonServcies;
        private readonly ServiceTable _serviceTable;
        private readonly ServiceCallSiteResolver _serviceCallSiteResolver;
        internal readonly ServiceResolver _root;

        public ServiceResolver(IServiceContainer serviceContainer)
        {
            _serviceTable = new ServiceTable(serviceContainer.Configuration);
            _serviceTable.Populate(serviceContainer);
            _resolvedScopedServcies = new ConcurrentDictionary<ServiceDefinition, object>();
            _resolvedSingletonServcies = new ConcurrentDictionary<ServiceDefinition, object>();
            _serviceCallSiteResolver = new ServiceCallSiteResolver(_serviceTable);
        }

        public ServiceResolver(ServiceResolver root)
        {
            _root = root;
            _serviceTable = root._serviceTable;
            _resolvedSingletonServcies = root._resolvedSingletonServcies;
            _resolvedScopedServcies = new ConcurrentDictionary<ServiceDefinition, object>();
            _serviceCallSiteResolver = new ServiceCallSiteResolver(_serviceTable);
        }

        public void Dispose()
        {
            if (_root == null || _root == this)
            {
                foreach (var singleton in _resolvedSingletonServcies)
                {
                    var disposable = singleton.Value as IDisposable;
                    disposable?.Dispose();
                }
            }
            foreach (var scoped in _resolvedScopedServcies)
            {
                var disposable = scoped.Value as IDisposable;
                disposable?.Dispose();
            }
        }

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            var definition = _serviceTable.TryGetService(serviceType);
            if (definition == null)
            {
                return null;
            }
            switch (definition.Lifetime)
            {
                case Lifetime.Singleton:
                    return _resolvedSingletonServcies.GetOrAdd(definition, d => _serviceCallSiteResolver.Resolve(d)(_root ?? this));
                case Lifetime.Scoped:
                    return _resolvedScopedServcies.GetOrAdd(definition, d => _serviceCallSiteResolver.Resolve(d)(this));
                default:
                    return _serviceCallSiteResolver.Resolve(definition)(this);
            }
        }
    }
}