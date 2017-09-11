using System;

namespace AspectCore.DynamicProxy
{
    public interface IAspectCachingProvider : IDisposable
    {
        IAspectCaching GetAspectCaching(string name);
    }
}