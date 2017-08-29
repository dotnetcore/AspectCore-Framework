using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IProxyGenerator
    {
        Type CreateInterfaceProxyType(Type serviceType, Type implementationType);

        Type CreateClassProxyType(Type serviceType, Type implementationType);
    }
}