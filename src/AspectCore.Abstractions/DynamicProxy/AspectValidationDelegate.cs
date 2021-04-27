using System.Reflection;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// 处理AspectValidationContext上下文的委托
    /// </summary>
    /// <param name="context">拦截验证上下文</param>
    /// <returns>true 需要被代理,否则false</returns>
    public delegate bool AspectValidationDelegate(AspectValidationContext context);
}
