using System;
using System.Linq;
using System.Reflection;
using AspectCore.Lite.Core.Descriptors;

namespace AspectCore.Lite.Core
{
    public sealed class Proxy : IMethodInvoker
    {
        private ParameterCollection parameterCollection;
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

        public void InjectionParameters(ParameterCollection parameterCollection)
        {
            if (parameterCollection == null)
            {
                throw new ArgumentNullException(nameof(parameterCollection));
            }
            this.parameterCollection = parameterCollection;
        }

        public object Invoke()
        {
            object[] args = parameterCollection.Select(p => p.Value).ToArray();
            return Method.Invoke(Instance, args);
        }
    }
}