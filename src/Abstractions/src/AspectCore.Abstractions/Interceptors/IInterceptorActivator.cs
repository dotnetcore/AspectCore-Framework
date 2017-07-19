using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorActivator
    {
        IInterceptor CreateInstance(Type interceptorType, object[] args);
    }
}
