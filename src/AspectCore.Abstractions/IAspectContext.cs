using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectContext : IDisposable
    {
        IServiceProvider ServiceProvider { get; }

        TargetDescriptor Target { get; }

        ProxyDescriptor Proxy { get; }

        ParameterCollection Parameters { get; }

        ParameterDescriptor ReturnParameter { get; }

        IDictionary<string, object> Items { get; }
    }
}