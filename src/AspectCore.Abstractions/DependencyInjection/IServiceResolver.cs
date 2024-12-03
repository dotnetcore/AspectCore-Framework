using System;
using AspectCore.DynamicProxy;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface IServiceResolver : IServiceProvider, IDisposable
#if NET8_0_OR_GREATER
        , Microsoft.Extensions.DependencyInjection.IKeyedServiceProvider
#endif
    {
        object Resolve(Type serviceType);
    }
}