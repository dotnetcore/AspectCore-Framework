using System;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class ReflectionTypedInterceptorActivator : ITypedInterceptorActivator
    {
        public IInterceptor CreateInstance(Type interceptorType, object[] args)
        {
            return (IInterceptor)Activator.CreateInstance(interceptorType, args);
        }
    }
}