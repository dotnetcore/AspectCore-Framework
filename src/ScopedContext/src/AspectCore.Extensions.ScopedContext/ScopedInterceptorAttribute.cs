using System;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.ScopedContext
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    [NonAspect]
    public abstract class ScopedInterceptorAttribute : InterceptorAttribute, IScopedInterceptor
    {
        public virtual ScopedOptions ScopedOption { get; set; } = ScopedOptions.None;
    }
}