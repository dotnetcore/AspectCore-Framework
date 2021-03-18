using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 执行验证管道以确定方法是否需要被代理
    /// </summary>
    [NonAspect]
    public interface IAspectValidator
    {
        /// <summary>
        /// 确定方法是否需要被代理
        /// </summary>
        /// <param name="method">待验证的方法</param>
        /// <param name="isStrictValidation">严格验证</param>
        /// <returns>验证是否成功，通过则表明使用者想为此方法生成代理</returns>
        bool Validate(MethodInfo method, bool isStrictValidation);
    }
}