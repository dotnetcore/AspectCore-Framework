using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    public interface IInterceptorInjector
    {
        void Inject(IInterceptor interceptor);
    }
}
