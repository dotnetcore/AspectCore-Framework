using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface IInterceptorInjectorProvider
    {
        IInterceptorInjector GetInjector(Type interceptorType);
    }
}
