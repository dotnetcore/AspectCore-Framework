using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IServiceContainer : IEnumerable<ServiceDefinition>
    {
        ILifetimeServiceContainer Singletons { get; }

        ILifetimeServiceContainer Scopeds { get; }

        ILifetimeServiceContainer Transients { get; }

        int Count { get; }

        void Add(ServiceDefinition item);

        bool Contains(Type serviceType);
    }
}