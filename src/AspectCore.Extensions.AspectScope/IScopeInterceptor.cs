using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    /// <summary>
    /// 拦截上下文只作用在标注此特性的元素上
    /// </summary>
    [NonAspect]
    public interface IScopeInterceptor : IInterceptor
    {
        Scope Scope { get; set; }
    }
}