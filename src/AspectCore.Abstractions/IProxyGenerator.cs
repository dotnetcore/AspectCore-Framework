using System;

namespace AspectCore.Abstractions
{
    public interface IProxyGenerator
    {
        Type CreateType(Type serviceType, Type implementationType);
    }
}
