using System;
using System.Collections.Concurrent;
using System.Linq;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
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
            _serviceCallSiteResolver = root._serviceCallSiteResolver;
            _resolvedScopedServcies = new ConcurrentDictionary<ServiceDefinition, object>();    
        }

        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        public object Resolve(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var definition = _serviceTable.TryGetService(serviceType);

            return ResolveDefinition(definition);
        }

        internal object ResolveDefinition(ServiceDefinition definition)
        {
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

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    if (_root == null || _root == this)
                    {
                        foreach (var singleton in _resolvedSingletonServcies.Where(x => x.Value != this))
                        {
                            var disposable = singleton.Value as IDisposable;
                            disposable?.Dispose();
                        }
                    }
                    foreach (var scoped in _resolvedScopedServcies.Where(x => x.Value != this))
                    {
                        var disposable = scoped.Value as IDisposable;
                        disposable?.Dispose();
                    }
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}