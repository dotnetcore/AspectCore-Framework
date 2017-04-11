using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public interface IProxyDescriptor
    {
        object ProxyInstance { get; }

        MethodInfo ProxyMethod { get; }

        Type ProxyType { get; }
    }
}
