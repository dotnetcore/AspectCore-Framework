using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public readonly struct AspectActivatorContext
    {
        public MethodInfo ServiceMethod { get; }

        public MethodInfo TargetMethod { get; }

        public MethodInfo ProxyMethod { get; }

        /// <summary>
        /// Gets the method used to evaluate configured <see cref="AspectCore.Configuration.AspectPredicate"/> filters.
        /// </summary>
        public MethodInfo PredicateMethod { get; }

        public object TargetInstance { get; }

        public object ProxyInstance { get; }

        public object[] Parameters { get; }

        public AspectActivatorContext(MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod, MethodInfo predicateMethod,
            object targetInstance, object proxyInstance, object[] parameters)
        {
            ServiceMethod = serviceMethod;
            TargetMethod = targetMethod;
            ProxyMethod = proxyMethod;
            PredicateMethod = predicateMethod;
            TargetInstance = targetInstance;
            ProxyInstance = proxyInstance;
            Parameters = parameters;
        }
    }
}