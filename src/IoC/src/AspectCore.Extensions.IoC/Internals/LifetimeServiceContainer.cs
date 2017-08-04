using System;
using System.Collections;
using System.Collections.Generic;
using AspectCore.Abstractions;
using System.Linq;

namespace AspectCore.Extensions.IoC.Internals
{
    internal sealed class LifetimeServiceContainer : ILifetimeServiceContainer
    {
        private readonly ICollection<IServiceDefinition> _internalCollection;

        public Lifetime Lifetime { get; }

        public LifetimeServiceContainer(ICollection<IServiceDefinition> collection, Lifetime lifetime)
        {
            _internalCollection = collection;
            Lifetime = lifetime;
        }

        public int Count => _internalCollection.Count(x => x.Lifetime == Lifetime);

        public void Add(IServiceDefinition item)
        {
            if (item.Lifetime == Lifetime)
            {
                _internalCollection.Add(item);
            }
        }

        public bool Contains(Type serviceType) => _internalCollection.Any(x => x.ServiceType == serviceType && x.Lifetime == Lifetime);

        public IEnumerator<IServiceDefinition> GetEnumerator() => _internalCollection.Where(x => x.Lifetime == Lifetime).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}