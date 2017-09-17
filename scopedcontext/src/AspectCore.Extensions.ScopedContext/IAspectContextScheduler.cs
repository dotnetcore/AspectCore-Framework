using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.ScopedContext
{
    [NonAspect]
    public interface IAspectContextScheduler
    {
        bool TryEnter(AspectContext context);

        void Release(AspectContext context);

        bool TryInclude(AspectContext context, IScopedInterceptor interceptor);

        AspectContext[] GetCurrentContexts();
    }
}
