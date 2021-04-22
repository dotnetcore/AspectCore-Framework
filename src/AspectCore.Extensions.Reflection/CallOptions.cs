namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 调用方式
    /// </summary>
    public enum CallOptions
    {
        /// <summary>
        /// 以声明类型调用
        /// </summary>
        Call,

        /// <summary>
        /// 以对象的真实类型调用
        /// </summary>
        Callvirt
    }
}
