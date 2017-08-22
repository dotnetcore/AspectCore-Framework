using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IServiceResolver : IServiceProvider, IDisposable
    {
        object Resolve(Type serviceType, string key);
    }
}
