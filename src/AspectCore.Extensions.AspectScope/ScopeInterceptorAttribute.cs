using System;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [NonAspect]
    public abstract class ScopeInterceptorAttribute : AbstractInterceptorAttribute, IScopeInterceptor
    {
        public virtual Scope Scope { get; set; } = Scope.None;
    }
}