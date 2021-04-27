namespace AspectCore.Extensions.Reflection
{
    /// <summary>
    /// 提供ParameterReflector
    /// </summary>
    public interface IParameterReflectorProvider
    {
        /// <summary>
        /// 参数反射操作对象数组
        /// </summary>
        ParameterReflector[] ParameterReflectors { get; }
    }
}