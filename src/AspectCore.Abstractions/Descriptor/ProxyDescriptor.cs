using AspectCore.Abstractions.Internal;
using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    public class ProxyDescriptor
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
