namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 提供CustomAttributeReflector对象
    /// </summary>
    public interface ICustomAttributeReflectorProvider
    {
        /// <summary>
        /// 自定义特性反射操作对象数组
        /// </summary>
        CustomAttributeReflector[] CustomAttributeReflectors { get; }
    }
}