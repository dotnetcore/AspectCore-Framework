using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 服务上下文
    /// </summary>
    public sealed class ServiceContext : IServiceContext
    {
        private readonly ICollection<ServiceDefinition> _collection;
        private readonly IAspectConfiguration _configuration;

        /// <summary>
        ///  服务上下文
        /// </summary>
        public ServiceContext()
            : this(null, null)
        {
        }

        /// <summary>
        /// 通过服务描述集合构造ServiceContext
        /// </summary>
        /// <param name="services">服务描述集合</param>
        public ServiceContext(IEnumerable<ServiceDefinition> services)
          : this(services, null)
        {
        }

        /// <summary>
        /// 通过配置构造ServiceContext
        /// </summary>
        /// <param name="aspectConfiguration">AspectCore配置</param>
        public ServiceContext(IAspectConfiguration aspectConfiguration)
           : this(null, aspectConfiguration)
        {
        }

        /// <summary>
        /// 组合服务描述集合和配置构造ServiceContext
        /// </summary>
        /// <param name="services">服务描述集合</param>
        /// <param name="aspectConfiguration">AspectCore配置</param>
        public ServiceContext(IEnumerable<ServiceDefinition> services, IAspectConfiguration aspectConfiguration)
        {
            _collection = new List<ServiceDefinition>();

            Singletons = new LifetimeServiceContext(_collection, Lifetime.Singleton);
            Scopeds = new LifetimeServiceContext(_collection, Lifetime.Scoped);
            Transients = new LifetimeServiceContext(_collection, Lifetime.Transient);

            if (services != null)
            {
                var configuration = services.LastOrDefault(x => x.ServiceType == typeof(IAspectConfiguration) && x is InstanceServiceDefinition);
                if (configuration != null)
                {
                    _configuration = (IAspectConfiguration)((InstanceServiceDefinition)configuration).ImplementationInstance;
                }
                foreach (var service in services)
                    _collection.Add(service);
            }

            if (aspectConfiguration != null)
            {
                _configuration = aspectConfiguration;
            }

            if (_configuration == null)
            {
                _configuration = new AspectConfiguration();
            }

            AddInternalServices();
        }

        /// <summary>
        /// 添加内置服务
        /// </summary>
        private void AddInternalServices()
        {
            Scopeds.AddDelegate<IServiceResolver>(resolver => resolver);

            if (!Contains(typeof(IServiceProvider)))
                Scopeds.AddDelegate<IServiceProvider>(resolver => resolver);
            if (!Contains(typeof(IPropertyInjectorFactory)))
                Scopeds.AddType<IPropertyInjectorFactory, PropertyInjectorFactory>(); 
            if (!Contains(typeof(IScopeResolverFactory)))
                Scopeds.AddDelegate<IScopeResolverFactory>(resolver => new ScopeResolverFactory(resolver));
            Singletons.AddInstance<IAspectConfiguration>(_configuration);
            if (!Contains(typeof(ITransientServiceAccessor<>)))
                Singletons.AddType(typeof(ITransientServiceAccessor<>), typeof(TransientServiceAccessor<>));
            
            //add service resolve callbacks
            Scopeds.AddType<IServiceResolveCallback, PropertyInjectorCallback>();

            //add DynamicProxy services   
            Singletons.AddType<IInterceptorSelector, ConfigureInterceptorSelector>();
            Singletons.AddType<IInterceptorSelector, AttributeInterceptorSelector>();
            Singletons.AddType<IAdditionalInterceptorSelector, AttributeAdditionalInterceptorSelector>();
            if (!Contains(typeof(IInterceptorCollector)))
                Singletons.AddType<IInterceptorCollector, InterceptorCollector>();
            if (!Contains(typeof(IAspectValidatorBuilder)))
                Singletons.AddType<IAspectValidatorBuilder, AspectValidatorBuilder>();
            if (!Contains(typeof(IAspectContextFactory)))
                Scopeds.AddType<IAspectContextFactory, AspectContextFactory>();
            if (!Contains(typeof(IAspectBuilderFactory)))
                Singletons.AddType<IAspectBuilderFactory, AspectBuilderFactory>();
            if (!Contains(typeof(IAspectActivatorFactory)))
                Scopeds.AddType<IAspectActivatorFactory, AspectActivatorFactory>();
            if (!Contains(typeof(IProxyGenerator)))
                Scopeds.AddType<IProxyGenerator, ProxyGenerator>();
            if (!Contains(typeof(IProxyTypeGenerator)))
                Singletons.AddType<IProxyTypeGenerator, ProxyTypeGenerator>();
            if (!Contains(typeof(IParameterInterceptorSelector)))
                Scopeds.AddType<IParameterInterceptorSelector, ParameterInterceptorSelector>();
            if (!Contains(typeof(IAspectCachingProvider)))
                Singletons.AddType<IAspectCachingProvider, AspectCachingProvider>();
            if (!Contains(typeof(IAspectExceptionWrapper)))
                Singletons.AddType<IAspectExceptionWrapper, AspectExceptionWrapper>();
        }

        /// <summary>
        /// 服务描述对象的数量
        /// </summary>
        public int Count => _collection.Count;

        /// <summary>
        /// 单例生命周期的服务集合
        /// </summary>
        public ILifetimeServiceContext Singletons { get; }

        /// <summary>
        /// 作用域生命周期的服务集合
        /// </summary>
        public ILifetimeServiceContext Scopeds { get; }

        /// <summary>
        /// 瞬时生命周期的服务集合
        /// </summary>
        public ILifetimeServiceContext Transients { get; }

        /// <summary>
        /// AspectCore配置
        /// </summary>
        public IAspectConfiguration Configuration => _configuration;

        /// <summary>
        /// 添加服务描述对象
        /// </summary>
        /// <param name="item">服务描述对象</param>
        public void Add(ServiceDefinition item) => _collection.Add(item);

        /// <summary>
        /// 移除服务描述对象
        /// </summary>
        /// <param name="item">服务描述对象</param>
        /// <returns>移除是否成功</returns>
        public bool Remove(ServiceDefinition item) => _collection.Remove(item);

        /// <summary>
        /// 容器中是否包含此类型的服务
        /// </summary>
        /// <param name="serviceType">服务</param>
        /// <returns>是否包含</returns>
        public bool Contains(Type serviceType) => _collection.Any(x => x.ServiceType == serviceType);

        public IEnumerator<ServiceDefinition> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }
}
