using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    public class ServiceContainer : IServiceContainer
    {
        private readonly ICollection<ServiceDefinition> _collection;
        private readonly IAspectConfiguration _configuration;

        public ServiceContainer()
            : this(null)
        {
        }

        public ServiceContainer(IEnumerable<ServiceDefinition> services)
        {
            _collection = new List<ServiceDefinition>();
            _configuration = new AspectConfiguration(this);

            Singletons = new LifetimeServiceContainer(_collection, Lifetime.Singleton);
            Scopeds = new LifetimeServiceContainer(_collection, Lifetime.Scoped);
            Transients = new LifetimeServiceContainer(_collection, Lifetime.Transient);

            if (services != null)
                foreach (var service in services)
                    _collection.Add(service);

            AddInternalServices();
        }

        private void AddInternalServices()
        {
            if (!Contains(typeof(IServiceProvider)))
                Scopeds.AddDelegate<IServiceProvider>(resolver => resolver);
            if (!Contains(typeof(IPropertyInjectorFactory)))
                Scopeds.AddType<IPropertyInjectorFactory, PropertyInjectorFactory>();
            Scopeds.AddDelegate<IServiceResolver>(resolver => resolver);
            Scopeds.AddDelegate<IScopeResolverFactory>(resolver => new ScopeResolverFactory(resolver));
            Singletons.AddInstance<IAspectConfiguration>(_configuration);

            //add DynamicProxy services   
            Singletons.AddType<IInterceptorSelector, MethodInterceptorSelector>();
            Singletons.AddType<IInterceptorSelector, TypeInterceptorSelector>();
            Singletons.AddType<IInterceptorSelector, ConfigureInterceptorSelector>();
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
        }

        public int Count => _collection.Count;

        public ILifetimeServiceContainer Singletons { get; }

        public ILifetimeServiceContainer Scopeds { get; }

        public ILifetimeServiceContainer Transients { get; }

        public IAspectConfiguration Configuration => _configuration;

        public void Add(ServiceDefinition item) => _collection.Add(item);

        public bool Contains(Type serviceType) => _collection.Any(x => x.ServiceType == serviceType);

        public IEnumerator<ServiceDefinition> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }
}
