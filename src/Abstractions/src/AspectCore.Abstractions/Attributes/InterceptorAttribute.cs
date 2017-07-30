using System;
using System.Threading.Tasks;

namespace AspectCore.Abstractions
{
    /// <summary>
    /// Standard interceptor definition via custom attribute.
    /// </summary>
    [NonAspect]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public abstract class InterceptorAttribute : Attribute, IInterceptor
    {
        public virtual bool AllowMultiple { get; } = false;

        public virtual int Order { get; set; } = 0;

        public bool Inherited { get; set; } = false;

        public virtual Task Invoke(AspectContext context, AspectDelegate next) => next(context);
    }
}