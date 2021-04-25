namespace AspectCore.Extensions.DataValidation
{
    /// <summary>
    /// 数据校验状态
    /// </summary>
    public enum DataValidationState
    {
        /// <summary>
        /// 未校验
        /// </summary>
        Unvalidated,

        /// <summary>
        /// 无效的
        /// </summary>
        Invalid,

        /// <summary>
        /// 已校验
        /// </summary>
        Valid,

        /// <summary>
        /// 跳过
        /// </summary>
        Skipped
    }
}