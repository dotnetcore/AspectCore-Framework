using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface ITargetServiceProvider
    {
        object GetTarget(Type serviceType);
    }
}
