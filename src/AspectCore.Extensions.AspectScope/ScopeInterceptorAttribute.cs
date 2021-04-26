using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    /// <summary>
    /// 拦截上下文只作用在标注此特性的元素上
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [NonAspect]
    public abstract class ScopeInterceptorAttribute : AbstractInterceptorAttribute, IScopeInterceptor
    {
        public virtual Scope Scope { get; set; } = Scope.None;
    }
}