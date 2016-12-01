using System;

namespace AspectCore.Lite.Abstractions
{
    public interface IAspectContext : IDisposable
    {
        IServiceProvider ApplicationServices { get; }

        IServiceProvider AspectServices { get; }

        Target Target { get; }

        Proxy Proxy { get; }

        ParameterCollection Parameters { get; }

        ParameterDescriptor ReturnParameter { get; }

        object State { get; set; }
    }
}
