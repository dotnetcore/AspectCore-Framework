using AspectCore.Lite.Abstractions;
using System;

namespace AspectCore.Lite.DynamicProxy
{
    [NonAspect]
    public interface ITargetServiceProvider : IDisposable
    {
        object GetTarget(Type serviceType);
    }
}
