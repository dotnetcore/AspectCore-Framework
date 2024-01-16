using System;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.DependencyInjection
{
    [NonAspect]
    public interface IServiceResolver : IServiceProvider, IDisposable
#if NET8_0_OR_GREATER
        , IKeyedServiceProvider
#endif
    {
        object Resolve(Type serviceType);
    }
}