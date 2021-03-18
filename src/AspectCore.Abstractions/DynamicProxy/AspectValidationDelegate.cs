using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 处理拦截验证上下文的委托
    /// </summary>
    /// <param name="context">拦截验证上下文</param>
    /// <returns>验证结果，验证通过则生成动态代理</returns>
    public delegate bool AspectValidationDelegate(AspectValidationContext context);
}
