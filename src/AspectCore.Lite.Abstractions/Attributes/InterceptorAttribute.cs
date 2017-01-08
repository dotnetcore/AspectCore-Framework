using System;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public abstract class InterceptorAttribute : Attribute, IInterceptor
    {
        public virtual bool AllowMultiple { get; } = false;

        public virtual int Order { get; set; } = 0;

        public abstract Task Invoke(IAspectContext context, AspectDelegate next);
    }
}
