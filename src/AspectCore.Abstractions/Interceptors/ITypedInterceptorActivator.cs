using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface ITypedInterceptorActivator
    {
        IInterceptor CreateInstance(Type interceptorType, object[] args);
    }
}
