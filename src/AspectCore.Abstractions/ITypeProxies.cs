using System;

namespace AspectCore.Abstractions
{
    public interface ITypeProxies
    {
        IServiceProvider ServiceProvider { get; }

        object ServiceInstance { get; }
    }
}
