using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IServiceProviderAccessor
    {
        IServiceProvider ServiceProvider { get; }
    }
}