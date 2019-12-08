using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface IServiceResolver : IServiceProvider, IDisposable
    {
        object Resolve(Type serviceType);
    }
}