using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface ILifetimeServiceContainer : IEnumerable<IServiceDefinition>
    {
        Lifetime Lifetime { get; }

        int Count { get; }

        void Add(IServiceDefinition item);

        bool Contains(Type serviceType);
    }
}