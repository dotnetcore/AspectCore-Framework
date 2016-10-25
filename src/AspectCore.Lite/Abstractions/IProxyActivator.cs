using System;

namespace AspectCore.Lite.Abstractions
{
    public interface IProxyActivator
    {
        object CreateInterfaceProxy(Type serviceType , object instance , params Type[] interfaceTypes);

        object CreateClassProxy(Type serviceType , object instance , params Type[] interfaceTypes);
    }
}
