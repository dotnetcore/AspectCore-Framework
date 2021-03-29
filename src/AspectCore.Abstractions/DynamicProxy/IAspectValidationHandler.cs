using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 验证管道由多个IAspectValidationHandler构建起来,
    /// 最终通过管道以确定方法是否需要被代理
    /// </summary>
    [NonAspect]
    public interface IAspectValidationHandler
    {
        /// <summary>
        /// 排序号，表示处理验证的顺序
        /// </summary>
        int Order { get; }

        /// <summary>
        /// 检查是否可被代理
        /// </summary>
        /// <param name="context">上下文</param>
        /// <param name="next">后续的验证处理委托</param>
        /// <returns>结果</returns>
        bool Invoke(AspectValidationContext context, AspectValidationDelegate next);
    }
}
