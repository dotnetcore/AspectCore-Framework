using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    public interface IProxyActivator
    {
        object CreateInterfaceProxy(Type serviceType , object instance , Type[] interfaceTypes);

        object CreateClassProxy(Type serviceType , object instance , Type[] interfaceTypes);
    }
}
