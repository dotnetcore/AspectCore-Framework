using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface ITypedInterceptorActivator
    {
        IInterceptor CreateInstance(Type interceptorType, object[] args);
    }
}
