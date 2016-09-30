using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public sealed class Proxy : IMethodInvoker
    {
        internal ParameterCollection ParameterCollection { get; set; }
        public object Instance { get; }
        public MethodInfo Method { get; }
        public Type ProxyType { get; }

        internal Proxy(object instance, MethodInfo method, Type proxyType)
        {
            if (Instance == null)
                throw new ArgumentNullException(nameof(Instance));

            if (method == null)
                throw new ArgumentNullException(nameof(method));

            if (proxyType == null)
                throw new ArgumentNullException(nameof(proxyType));

            Instance = instance;
            Method = method;
            ProxyType = proxyType;
        }

        public object Invoke()
        {
            object[] args = ParameterCollection?.Select(p => p.Value)?.ToArray();
            return Method.Invoke(Instance, args);
        }
    }
}