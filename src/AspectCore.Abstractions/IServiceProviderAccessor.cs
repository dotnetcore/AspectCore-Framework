using System;

namespace AspectCore.Abstractions
{
    public interface IServiceProviderAccessor
    {
        IServiceProvider ServiceProvider { get; }
    }
}