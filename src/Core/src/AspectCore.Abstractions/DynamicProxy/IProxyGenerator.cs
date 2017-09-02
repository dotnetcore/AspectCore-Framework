using System;

namespace AspectCore.DynamicProxy
{
    public interface IProxyGenerator
    {
        object CreateInterfaceProxy(Type serviceType);

        object CreateInterfaceProxy(Type serviceType, object implementationInstance);

        object CreateClassProxy(Type serviceType, object implementationInstance);
    }
}