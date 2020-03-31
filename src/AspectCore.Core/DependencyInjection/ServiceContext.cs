using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;

namespace AspectCore.DependencyInjection
{
    public sealed class ServiceContext : IServiceContext
    {
        private readonly ICollection<ServiceDefinition> _collection;
        private readonly IAspectConfiguration _configuration;

        public ServiceContext()
            : this(null, null)
        {
        }

        public ServiceContext(IEnumerable<ServiceDefinition> services)
          : this(services, null)
        {
        }

        public ServiceContext(IAspectConfiguration aspectConfiguration)
           : this(null, aspectConfiguration)
        {
        }

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

        public int Count => _collection.Count;

        public ILifetimeServiceContext Singletons { get; }

        public ILifetimeServiceContext Scopeds { get; }

        public ILifetimeServiceContext Transients { get; }

        public IAspectConfiguration Configuration => _configuration;

        public void Add(ServiceDefinition item) => _collection.Add(item);

        public bool Remove(ServiceDefinition item) => _collection.Remove(item);

        public bool Contains(Type serviceType) => _collection.Any(x => x.ServiceType == serviceType);

        public IEnumerator<ServiceDefinition> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }
}
