using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Injector
{
    [NonAspect]
    public interface IServiceResolver : IServiceProvider, IDisposable
    {
        object Resolve(Type serviceType);
    }
}