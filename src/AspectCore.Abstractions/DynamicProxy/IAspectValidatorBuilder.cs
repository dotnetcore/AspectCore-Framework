namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 构建检查管道
    /// </summary>
    [NonAspect]
    public interface IAspectValidatorBuilder
    {
        /// <summary>
        /// 构建检查管道
        /// </summary>
        /// <returns>标识检查管道</returns>
        IAspectValidator Build();
    }
}
