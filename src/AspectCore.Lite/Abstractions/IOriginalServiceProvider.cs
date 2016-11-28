using System;

namespace AspectCore.Lite.Abstractions
{
    public interface IOriginalServiceProvider
    {
        object GetService(Type serviceType);
    }
}