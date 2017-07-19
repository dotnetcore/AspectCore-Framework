using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IProxyDescriptor
    {
        object ProxyInstance { get; }

        MethodInfo ProxyMethod { get; }

        Type ProxyType { get; }
    }
}