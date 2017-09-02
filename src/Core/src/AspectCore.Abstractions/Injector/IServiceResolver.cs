using System;

namespace AspectCore.Injector
{
    public interface IServiceResolver : IServiceProvider, IDisposable
    {
        object Resolve(Type serviceType);
    }
}