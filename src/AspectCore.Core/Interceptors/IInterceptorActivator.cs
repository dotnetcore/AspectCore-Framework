using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface IInterceptorActivator
    {
        IInterceptor CreateInstance(Type interceptorType, object[] args);
    }
}
