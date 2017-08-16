using System;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    public class ProxyDescriptor : IProxyDescriptor
    {
        public virtual object ProxyInstance { get; }

        public virtual MethodInfo ProxyMethod { get; }

        public virtual Type ProxyType { get; }

        public ProxyDescriptor(object proxyInstance, MethodInfo proxyMethod, Type proxyType)
        {
            ProxyType = proxyType;
            ProxyMethod = proxyMethod;
            ProxyInstance = proxyInstance;
        }
    }
}
