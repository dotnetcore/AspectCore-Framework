using System.Threading.Tasks;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public abstract class AbstractInterceptor : IInterceptor
    {
        public virtual bool AllowMultiple { get; } = false;

        public virtual int Order { get; set; } = 0;

        public bool Inherited { get; set; } = false;

        public abstract Task Invoke(AspectContext context, AspectDelegate next);
    }
}