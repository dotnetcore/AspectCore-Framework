using System;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAspectCachingProvider : IDisposable
    {
        IAspectCaching GetAspectCaching(string name);
    }
}