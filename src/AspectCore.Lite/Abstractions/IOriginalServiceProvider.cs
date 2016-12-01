using System;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IOriginalServiceProvider
    {
        object GetService(Type serviceType);
    }
}