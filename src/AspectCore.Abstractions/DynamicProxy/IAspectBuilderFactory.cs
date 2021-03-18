namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 拦截管道构建者的工厂接口
    /// </summary>
    [NonAspect]
    public interface IAspectBuilderFactory
    {
        /// <summary>
        /// 创建一个拦截管道构建者
        /// </summary>
        /// <param name="context">拦截上下文</param>
        /// <returns>拦截管道构建者</returns>
        IAspectBuilder Create(AspectContext context);
    }
}
