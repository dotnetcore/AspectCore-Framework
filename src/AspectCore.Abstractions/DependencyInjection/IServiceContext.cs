using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface IServiceContext : IEnumerable<ServiceDefinition>
    {
        ILifetimeServiceContext Singletons { get; }

        ILifetimeServiceContext Scopeds { get; }

        ILifetimeServiceContext Transients { get; }

        IAspectConfiguration Configuration { get; }

        int Count { get; }

        void Add(ServiceDefinition item);

        bool Remove(ServiceDefinition item);

        bool Contains(Type serviceType);
    }
}