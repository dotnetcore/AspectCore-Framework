using System;
using System.Collections.Generic;
using System.Text;

namespace AspectCore.Abstractions
{

    [NonAspect]
    public interface IProxyTypeGenerator
    {
        Type CreateInterfaceProxyType(Type serviceType);

        Type CreateInterfaceProxyType(Type serviceType, Type implementationType);

        Type CreateClassProxyType(Type serviceType, Type implementationType);
    }
}