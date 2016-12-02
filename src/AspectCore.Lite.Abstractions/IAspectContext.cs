using System;

namespace AspectCore.Lite.Abstractions
{
    public interface IAspectContext : IDisposable
    {
        IServiceProvider ServiceProvider { get; }

        TargetDescriptor Target { get; }

        ProxyDescriptor Proxy { get; }

        ParameterCollection Parameters { get; }

        ParameterDescriptor ReturnParameter { get; }

        Object AspectData { get; set; }
    }
}