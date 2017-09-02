using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;

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
            Scopeds.AddDelegate<IServiceResolver>(resolver => resolver);
            Scopeds.AddDelegate<IScopeResolverFactory>(resolver => new ScopeResolverFactory(resolver));
            Singletons.AddInstance<IAspectConfiguration>(_configuration);
        }

        public int Count => _collection.Count;

        public ILifetimeServiceContainer Singletons { get; }

        public ILifetimeServiceContainer Scopeds { get; }

        public ILifetimeServiceContainer Transients { get; }

        public IAspectConfiguration Configuration => throw new NotImplementedException();

        public void Add(ServiceDefinition item) => _collection.Add(item);

        public bool Contains(Type serviceType) => _collection.Any(x => x.ServiceType == serviceType);

        public IEnumerator<ServiceDefinition> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator();
    }
}
