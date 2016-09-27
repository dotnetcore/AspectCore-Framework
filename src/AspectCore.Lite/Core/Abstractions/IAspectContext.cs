using AspectCore.Lite.Core.Descriptors;
using System;

namespace AspectCore.Lite.Core
{
    public interface IAspectContext : IDisposable
    {
        IServiceProvider ApplicationServices { get; }
        IServiceProvider AspectServices { get; }
        Target Target { get; }
        Proxy Proxy { get; }
        ParameterCollection Parameters { get; }
        ParameterDescriptor ReturnParameter { get; }
    }
}
