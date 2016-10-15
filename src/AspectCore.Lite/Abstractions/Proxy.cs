using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public sealed class Proxy
    {
        public object Instance { get; }
        public MethodInfo Method { get; }
        public Type ProxyType { get; }

        internal Proxy(object instance, MethodInfo method, Type proxyType)
        {
            if (instance == null)
                throw new ArgumentNullException(nameof(instance));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (proxyType == null)
                throw new ArgumentNullException(nameof(proxyType));

            Instance = instance;
            Method = method;
            ProxyType = proxyType;
        }
    }
}