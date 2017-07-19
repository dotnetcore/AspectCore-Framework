using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IServiceInstanceProvider
    {
        object GetInstance(Type serviceType);
    }
}
