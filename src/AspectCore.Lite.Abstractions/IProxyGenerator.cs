using System;

namespace AspectCore.Lite.Abstractions
{
    public interface IProxyGenerator
    {
        Type CreateType(Type serviceType, Type implementationType);
    }
}
