using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IProxyGenerator
    {
        Type CreateInterfaceProxyType(Type serviceType, Type implementationType, params Type[] interfaces);

        Type CreateClassProxyType(Type serviceType, Type implementationType, params Type[] interfaces);
    }
}