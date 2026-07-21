using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    internal sealed class ServiceResolver : IServiceResolver, IServiceResolveCallbackProvider
    {
        private readonly ConcurrentDictionary<ServiceDefinition, object> _resolvedScopedServices;
        private readonly ConcurrentDictionary<ServiceDefinition, Lazy<object>> _resolvedSingletonServices;
        private readonly ServiceTable _serviceTable;
        private readonly ServiceCallSiteResolver _serviceCallSiteResolver;
        internal readonly ServiceResolver _root;

        public ServiceResolver(IServiceContext serviceContext)
        {
            _serviceTable = new ServiceTable(serviceContext);
            _serviceTable.Populate(serviceContext);
            _resolvedScopedServices = new ConcurrentDictionary<ServiceDefinition, object>();
            _resolvedSingletonServices = new ConcurrentDictionary<ServiceDefinition, Lazy<object>>();
            _serviceCallSiteResolver = new ServiceCallSiteResolver(_serviceTable);
            ServiceResolveCallbacks = this.ResolveMany<IServiceResolveCallback>().ToArray();
        }

        public IServiceResolveCallback[] ServiceResolveCallbacks { get; }

        public ServiceResolver(ServiceResolver root)
        {
            _root = root;
            _serviceTable = root._serviceTable;
            _resolvedSingletonServices = root._resolvedSingletonServices;
            _serviceCallSiteResolver = root._serviceCallSiteResolver;
            _resolvedScopedServices = new ConcurrentDictionary<ServiceDefinition, object>();
            ServiceResolveCallbacks = this.ResolveMany<IServiceResolveCallback>().ToArray();
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
                    var lazy = _resolvedSingletonServices.GetOrAdd(definition,
                        static (d, state) => new Lazy<object>(() => state.callSiteResolver.Resolve(d)(state.resolver)),
                        (callSiteResolver: _serviceCallSiteResolver, resolver: _root ?? this));
                    return lazy.Value;
                case Lifetime.Scoped:
                    if (_resolvedScopedServices.TryGetValue(definition, out var scoped))
                    {
                        return scoped;
                    }
                    return _resolvedScopedServices.GetOrAdd(definition, static (d, state) => state.callSiteResolver.Resolve(d)(state.resolver), (callSiteResolver: _serviceCallSiteResolver, resolver: this));
                default:
                    return _serviceCallSiteResolver.Resolve(definition)(this);
            }
        }

#if NET8_0_OR_GREATER
        public object GetKeyedService(Type serviceType, object serviceKey)
        {
            var definition = _serviceTable.TryGetService(serviceType, serviceKey);
            return ResolveDefinition(definition);
        }

        public object GetRequiredKeyedService(Type serviceType, object serviceKey)
        {
            var definition = _serviceTable.TryGetService(serviceType, serviceKey);
            var service = ResolveDefinition(definition);
            if (service == null)
            {
                throw new InvalidOperationException($"No service for type '{serviceType}' with key '{serviceKey}' has been registered.");
            }
            return service;
        }
#endif

        #region IDisposable Support
        private bool disposedValue = false;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposedValue = true;
                    if (_root == null || _root == this)
                    {
                        foreach (var singleton in _resolvedSingletonServices.Where(x => x.Value.IsValueCreated && x.Value.Value != this))
                        {
                            var disposable = singleton.Value.Value as IDisposable;
                            disposable?.Dispose();
                        }
                    }
                    foreach (var scoped in _resolvedScopedServices.Where(x => x.Value != this))
                    {
                        var disposable = scoped.Value as IDisposable;
                        disposable?.Dispose();
                    }
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
