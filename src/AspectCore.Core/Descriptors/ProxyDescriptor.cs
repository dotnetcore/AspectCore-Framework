using System;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    internal class ProxyDescriptor : IProxyDescriptor
    {
        public virtual object ProxyInstance { get; }
        public virtual MethodInfo ProxyMethod { get; }
        public virtual Type ProxyType { get; }

        public ProxyDescriptor(object proxyInstance, MethodInfo proxyMethod, Type proxyType)
        {
            if (proxyInstance == null)
            {
                throw new ArgumentNullException(nameof(proxyInstance));
            }
            if (proxyMethod == null)
            {
                throw new ArgumentNullException(nameof(proxyMethod));
            }
            if (proxyType == null)
            {
                throw new ArgumentNullException(nameof(proxyType));
            }

            ProxyType = proxyType;
            ProxyMethod = proxyMethod;
            ProxyInstance = proxyInstance;
            ProxyMethod = proxyMethod.ReacquisitionIfDeclaringTypeIsGenericTypeDefinition(proxyType);
        }
    }
}
