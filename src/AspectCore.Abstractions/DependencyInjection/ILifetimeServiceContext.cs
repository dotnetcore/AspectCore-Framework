using System;
using System.Collections.Generic;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface ILifetimeServiceContext : IEnumerable<ServiceDefinition>
    {
        Lifetime Lifetime { get; }

        int Count { get; }

        void Add(ServiceDefinition item);

        bool Contains(Type serviceType);
    }
}