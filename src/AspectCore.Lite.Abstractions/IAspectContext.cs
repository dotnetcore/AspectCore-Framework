using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IAspectContext
    {
        IServiceProvider ServiceProvider { get; }

        TargetDescriptor Target { get; }

        ProxyDescriptor Proxy { get; }

        ParameterCollection Parameters { get; }

        ParameterDescriptor ReturnParameter { get; }

        object AspectData { get; set; }
    }
}