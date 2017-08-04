using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IServiceContainer : IEnumerable<IServiceDefinition>
    {
        ILifetimeServiceContainer Singletons { get; }

        ILifetimeServiceContainer Scopeds { get; }

        ILifetimeServiceContainer Transients { get; }

        int Count { get; }

        void Add(IServiceDefinition item);

        bool Contains(Type serviceType);
    }
}