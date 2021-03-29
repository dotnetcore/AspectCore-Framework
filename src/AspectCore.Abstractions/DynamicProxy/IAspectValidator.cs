using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 执行检查管道以确定方法是否需要被代理
    /// </summary>
    [NonAspect]
    public interface IAspectValidator
    {
        /// <summary>
        /// 确定方法是否需要被代理
        /// </summary>
        /// <param name="method">待检查的方法</param>
        /// <param name="isStrictValidation">检查模式</param>
        /// <returns>true 需要代理 false 不需要</returns>
        bool Validate(MethodInfo method, bool isStrictValidation);
    }
}