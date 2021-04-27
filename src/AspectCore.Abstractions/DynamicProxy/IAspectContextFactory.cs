namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 创建拦截上下文的工厂接口
    /// </summary>
    [NonAspect]
    public interface IAspectContextFactory
    {
        /// <summary>
        /// 创建一个拦截上下文
        /// </summary>
        /// <param name="activatorContext">用于切面调用过程内部传递切面的上下文信息</param>
        /// <returns>拦截上下文</returns>
        AspectContext CreateContext(AspectActivatorContext activatorContext);

        /// <summary>
        /// 释放拦截上下文对象中的非托管资源
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        void ReleaseContext(AspectContext aspectContext);
    }
}
