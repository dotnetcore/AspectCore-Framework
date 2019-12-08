using System;
using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    /// <summary>
    /// Standard interceptor definition via custom attribute.
    /// </summary>
    [NonAspect]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, Inherited = false)]
    public abstract class AbstractInterceptorAttribute : Attribute, IInterceptor
    {
        public virtual bool AllowMultiple { get; } = false;

        public virtual int Order { get; set; } = 0;

        public bool Inherited { get; set; } = false;

        public abstract Task Invoke(AspectContext context, AspectDelegate next);
    }
}