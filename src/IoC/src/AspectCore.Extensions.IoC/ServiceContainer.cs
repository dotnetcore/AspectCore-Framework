using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Abstractions;
using AspectCore.Extensions.IoC.Internals;

namespace AspectCore.Extensions.IoC
{
    public class ServiceContainer : IServiceContainer
    {
        private readonly ICollection<IServiceDefinition> _collection;

        public ServiceContainer()
            : this(null)
        {
        }

        public ServiceContainer(IEnumerable<IServiceDefinition> services)
        {
            _collection = new List<IServiceDefinition>();

            Singletons = new LifetimeServiceContainer(_collection, Lifetime.Singleton);
            Scopeds = new LifetimeServiceContainer(_collection, Lifetime.Scoped);
            Transients = new LifetimeServiceContainer(_collection, Lifetime.Transient);

            if (services != null)
                foreach (var service in services)
                    _collection.Add(service);

        }

        public int Count => _collection.Count;

        public ILifetimeServiceContainer Singletons { get; }

        public ILifetimeServiceContainer Scopeds { get; }

        public ILifetimeServiceContainer Transients { get; }

        public void Add(IServiceDefinition item) => _collection.Add(item);

        public bool Contains(Type serviceType) => _collection.Any(x => x.ServiceType == serviceType);

        public IEnumerator<IServiceDefinition> GetEnumerator() => _collection.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _collection.GetEnumerator(); 
    }
}