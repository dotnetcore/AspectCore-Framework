using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public struct AspectActivatorContext
    {
        private static readonly object[] emptyParameters = new object[0];

        public Type ServiceType { get; }

        public MethodInfo ServiceMethod { get; }

        public MethodInfo TargetMethod { get; }

        public MethodInfo ProxyMethod { get; }

        public object ServiceInstance { get; }

        public object ProxyInstance { get; }

        public object[] Parameters { get; }

        public AspectActivatorContext(Type serviceType, MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod, object serviceInstance, object proxyInstance, object[] parameters)
        {
            ServiceType = serviceType;
            ServiceMethod = serviceMethod;
            TargetMethod = targetMethod;
            ProxyMethod = proxyMethod;
            ServiceInstance = serviceInstance;
            ProxyInstance = proxyMethod;
            Parameters = parameters;
        }
    }
}