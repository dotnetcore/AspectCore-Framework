using AspectCore.Lite.Abstractions.Attributes;
using System;

namespace AspectCore.Lite.Abstractions.Resolution
{
    [NonAspect]
    public interface ISupportOriginalService : IDisposable
    {
        object GetService(Type serviceType);
    }
}
