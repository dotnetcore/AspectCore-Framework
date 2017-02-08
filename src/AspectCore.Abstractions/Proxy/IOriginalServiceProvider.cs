using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IOriginalServiceProvider
    {
        object GetService(Type serviceType);
    }
}
