using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class ReflectionInterceptorActivator : IInterceptorActivator
    {
        public IInterceptor CreateInstance(Type interceptorType, object[] args)
        {
            return (IInterceptor)Activator.CreateInstance(interceptorType, args);
        }
    }
}