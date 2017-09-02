using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public struct AspectActivatorContext
    {
        public MethodInfo ServiceMethod { get; }

        public MethodInfo TargetMethod { get; }

        public MethodInfo ProxyMethod { get; }

        public object ServiceInstance { get; }

        public object ProxyInstance { get; }

        public object[] Parameters { get; }

        public AspectActivatorContext(MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod, object serviceInstance, object proxyInstance, object[] parameters)
        {
            ServiceMethod = serviceMethod;
            TargetMethod = targetMethod;
            ProxyMethod = proxyMethod;
            ServiceInstance = serviceInstance;
            ProxyInstance = proxyMethod;
            Parameters = parameters;
        }
    }
}