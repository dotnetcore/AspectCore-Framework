using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorInjectorProvider
    {
        IInterceptorInjector GetInjector(Type interceptorType);
    }
}
