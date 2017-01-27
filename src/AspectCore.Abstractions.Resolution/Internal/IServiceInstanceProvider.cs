using System;

namespace AspectCore.Abstractions.Resolution.Internal
{
    [NonAspect]
    public interface IServiceInstanceProvider
    {
        object GetInstance(Type serviceType);
    }
}
