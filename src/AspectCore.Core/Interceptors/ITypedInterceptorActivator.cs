using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public interface ITypedInterceptorActivator
    {
        IInterceptor CreateInstance(Type interceptorType, object[] args);
    }
}
