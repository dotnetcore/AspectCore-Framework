using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public struct AspectActivatorContext
    {
        public Type ServiceType { get; }

        public MethodInfo ServiceMethod { get; }

        public MethodInfo ProxyMethod { get; }

        public object ProxyInstance { get; }

        public object[] Parameters { get; }

        public AspectActivatorContext(Type serviceType, MethodInfo serviceMethod, MethodInfo proxyMethod, object proxyInstance, object[] parameters)
        {
            ServiceType = serviceType;
            ServiceMethod = serviceMethod;
            ProxyMethod = proxyMethod;
            ProxyInstance = proxyMethod;
            Parameters = parameters;
        }
    }
}