using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface IServiceContainer : IEnumerable<ServiceDefinition>
    {
        ILifetimeServiceContainer Singletons { get; }

        ILifetimeServiceContainer Scopeds { get; }

        ILifetimeServiceContainer Transients { get; }

        IAspectConfiguration Configuration { get; }

        int Count { get; }

        void Add(ServiceDefinition item);

        bool Contains(Type serviceType);
    }
}