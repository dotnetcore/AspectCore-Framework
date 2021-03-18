using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 用于切面调用过程内部传递切面上下文信息
    /// </summary>
    public struct AspectActivatorContext
    {
        /// <summary>
        /// 暴露服务中对应的方法（目标对象中被拦截的方法实现的对应接口或父类的那个方法）
        /// </summary>
        public MethodInfo ServiceMethod { get; }

        /// <summary>
        /// 目标方法，即实现类中被拦截的方法
        /// </summary>
        public MethodInfo TargetMethod { get; }

        /// <summary>
        /// 代理方法
        /// </summary>
        public MethodInfo ProxyMethod { get; }

        /// <summary>
        /// 目标对象实例
        /// </summary>
        public object TargetInstance { get; }

        /// <summary>
        /// 代理对象实例
        /// </summary>
        public object ProxyInstance { get; }

        /// <summary>
        /// 被拦截方法的参数
        /// </summary>
        public object[] Parameters { get; }

        /// <summary>
        /// 构造AspectActivatorContext对象,用于切面调用过程内部传递切面上下文信息
        /// </summary>
        /// <param name="serviceMethod">服务方法</param>
        /// <param name="targetMethod">目标方法</param>
        /// <param name="proxyMethod">代理方法</param>
        /// <param name="targetInstance">目标对象</param>
        /// <param name="proxyInstance">代理实例</param>
        /// <param name="parameters">目标方法的参数</param>
        public AspectActivatorContext(MethodInfo serviceMethod, MethodInfo targetMethod, MethodInfo proxyMethod, 
            object targetInstance, object proxyInstance, object[] parameters)
        {
            ServiceMethod = serviceMethod;
            TargetMethod = targetMethod;
            ProxyMethod = proxyMethod;
            TargetInstance = targetInstance;
            ProxyInstance = proxyInstance;
            Parameters = parameters;
        }
    }
}