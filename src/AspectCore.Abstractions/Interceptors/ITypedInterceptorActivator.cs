using System;

namespace AspectCore.Abstractions
{
    public interface ITypedInterceptorActivator
    {
        IInterceptor CreateInstance(Type interceptorType, object[] args);
    }
}
