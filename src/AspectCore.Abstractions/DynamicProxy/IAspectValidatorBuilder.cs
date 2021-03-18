namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 验证管道的构建者接口
    /// </summary>
    [NonAspect]
    public interface IAspectValidatorBuilder
    {
        /// <summary>
        /// 构建验证管道
        /// </summary>
        /// <returns>标识验证管道</returns>
        IAspectValidator Build();
    }
}
