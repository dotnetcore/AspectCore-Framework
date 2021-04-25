using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public interface IScopeInterceptor : IInterceptor
    {
        /// <summary>
        /// 作用域
        /// </summary>
        Scope Scope { get; set; }
    }
}