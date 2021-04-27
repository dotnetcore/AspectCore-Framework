namespace AspectCore.DynamicProxy.Parameters
{
    /// <summary>
    /// 参数拦截上下文
    /// </summary>
    public struct ParameterAspectContext
    {
        /// <summary>
        /// 被拦截的参数
        /// </summary>
        public Parameter Parameter { get; }

        /// <summary>
        /// 拦截上下文
        /// </summary>
        public AspectContext AspectContext { get; }

        /// <summary>
        /// 构造一个参数拦截上下文对象
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="parameter">被拦截的参数</param>
        public ParameterAspectContext(AspectContext aspectContext, Parameter parameter)
        {
            AspectContext = aspectContext;
            Parameter = parameter;
        }
    }
}