using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 提供获取服务功能
    /// </summary>
    [NonAspect]
    internal sealed class ServiceResolver : IServiceResolver,IServiceResolveCallbackProvider
    {
        //作用域生命周期的缓存字典，描述了服务描述对象和服务之间的对应关系
        private readonly ConcurrentDictionary<ServiceDefinition, object> _resolvedScopedServices;
        //单例生命周期的缓存字典，描述了服务描述对象和服务之间的对应关系
        private readonly ConcurrentDictionary<ServiceDefinition, object> _resolvedSingletonServices;
        private readonly ServiceTable _serviceTable;
        private readonly ServiceCallSiteResolver _serviceCallSiteResolver;
        internal readonly ServiceResolver _root;

        /// <summary>
        /// 提供获取服务功能的对象
        /// </summary>
        /// <param name="serviceContext">服务上下文</param>
        public ServiceResolver(IServiceContext serviceContext)
        {
            _serviceTable = new ServiceTable(serviceContext.Configuration);
            _serviceTable.Populate(serviceContext);
            _resolvedScopedServices = new ConcurrentDictionary<ServiceDefinition, object>();
            _resolvedSingletonServices = new ConcurrentDictionary<ServiceDefinition, object>();
            _serviceCallSiteResolver = new ServiceCallSiteResolver(_serviceTable);
            ServiceResolveCallbacks = this.ResolveMany<IServiceResolveCallback>().ToArray();
        }

        /// <summary>
        /// 服务获取后的回调
        /// </summary>
        public IServiceResolveCallback[] ServiceResolveCallbacks { get; }

        /// <summary>
        /// 提供获取服务功能的对象
        /// </summary>
        /// <param name="ServiceResolver">根容器</param>
        public ServiceResolver(ServiceResolver root)
        {
            _root = root;
            _serviceTable = root._serviceTable;
            _resolvedSingletonServices = root._resolvedSingletonServices;
            _serviceCallSiteResolver = root._serviceCallSiteResolver;
            _resolvedScopedServices = new ConcurrentDictionary<ServiceDefinition, object>();  
            ServiceResolveCallbacks = this.ResolveMany<IServiceResolveCallback>().ToArray();
        }

        /// <summary>
        /// 通过服务的类型获取服务对象
        /// </summary>
        /// <param name="serviceType">服务的类型</param>
        /// <returns>服务对象</returns>
        public object GetService(Type serviceType)
        {
            return Resolve(serviceType);
        }

        /// <summary>
        /// 通过服务的类型获取服务对象
        /// </summary>
        /// <param name="serviceType">服务的类型</param>
        /// <returns>服务对象</returns>
        public object Resolve(Type serviceType)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            var definition = _serviceTable.TryGetService(serviceType);

            return ResolveDefinition(definition);
        }

        /// <summary>
        /// 通过服务的描述对象获取服务
        /// </summary>
        /// <param name="definition">服务的描述对象</param>
        /// <returns>服务对象</returns>
        internal object ResolveDefinition(ServiceDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            switch (definition.Lifetime)
            {
                case Lifetime.Singleton:
                    return _resolvedSingletonServices.GetOrAdd(definition, d => _serviceCallSiteResolver.Resolve(d)(_root ?? this));
                case Lifetime.Scoped:
                    return _resolvedScopedServices.GetOrAdd(definition, d => _serviceCallSiteResolver.Resolve(d)(this));
                default:
                    return _serviceCallSiteResolver.Resolve(definition)(this);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false;

        //释放未托管资源（注册的服务可能包含未托管资源）
        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    disposedValue = true;
                    if (_root == null || _root == this)
                    {
                        foreach (var singleton in _resolvedSingletonServices.Where(x => x.Value != this))
                        {
                            var disposable = singleton.Value as IDisposable;
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