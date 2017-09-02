using System;
using System.Collections.Generic;
using AspectCore.Configuration;

namespace AspectCore.Injector
{
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