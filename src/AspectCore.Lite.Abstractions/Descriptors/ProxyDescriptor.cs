using AspectCore.Lite.Abstractions.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public sealed class ProxyDescriptor
    {
        public object ProxyInstance { get; }
        public MethodInfo ProxyMethod { get; }
        public Type ProxyType { get; }

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
