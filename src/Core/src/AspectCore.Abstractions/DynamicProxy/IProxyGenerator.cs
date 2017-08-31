using System;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IProxyGenerator
    {
        object CreateInterfaceProxy(Type serviceType);

        object CreateInterfaceProxy(Type serviceType, object implementationInstance);

        object CreateClassProxy(Type serviceType, object implementationInstance);
    }
}