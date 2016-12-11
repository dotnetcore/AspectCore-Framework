using AspectCore.Lite.Abstractions;
using System;

namespace AspectCore.Lite.DynamicProxy.Resolution
{
    [NonAspect]
    public interface ISupportOriginalService : IDisposable
    {
        object GetService(Type serviceType);
    }
}
