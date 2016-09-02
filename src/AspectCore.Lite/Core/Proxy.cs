using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Lite.Core.Descriptors;

namespace AspectCore.Lite.Core
{
    public sealed class Proxy : IMethodInvoker
    {
        public object Instance { get; }
        public MethodInfo Method { get; }
        public Type ProxyType { get; }

        internal Proxy(object instance , MethodInfo method , Type proxyType)
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

        public object Invoke(ParameterCollection parameterCollection)
        {
            object[] args = parameterCollection.Select(p => p.Value).ToArray();
            return Method.Invoke(Instance , args);
        }
    }
}