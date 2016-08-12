using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Core
{
    public sealed class Proxy
    {
        public object Instance { get; }
        //public MethodInfo Method { get; }
        public Type ProxyType { get; }

        internal Proxy(object instance, /* MethodInfo method,*/ Type proxyType)
        {
            if (Instance == null)
                throw new ArgumentNullException(nameof(Instance));

            //if (method == null)
            //    throw new ArgumentNullException(nameof(method));

            if (proxyType == null)
                throw new ArgumentNullException(nameof(proxyType));

            Instance = instance;
            //Method = method;
            ProxyType = proxyType;
        }
    }
}