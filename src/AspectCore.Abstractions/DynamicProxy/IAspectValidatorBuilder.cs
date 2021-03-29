namespace AspectCore.DynamicProxy
{
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
