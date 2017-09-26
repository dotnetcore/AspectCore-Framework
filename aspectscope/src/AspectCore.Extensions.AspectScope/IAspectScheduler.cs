using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    [NonAspect]
    public interface IAspectScheduler
    {
        bool TryEnter(AspectContext context);

        void Release(AspectContext context);

        bool TryRelate(AspectContext context, IInterceptor interceptor);

        AspectContext[] GetCurrentContexts();
    }
}
