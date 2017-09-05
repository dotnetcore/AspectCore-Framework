using System;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface ILifetimeServiceContainer : IEnumerable<ServiceDefinition>
    {
        Lifetime Lifetime { get; }

        int Count { get; }

        void Add(ServiceDefinition item);

        bool Contains(Type serviceType);
    }
}