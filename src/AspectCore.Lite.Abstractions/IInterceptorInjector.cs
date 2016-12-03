namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptorInjector
    {
        void Inject(IInterceptor interceptor);
    }
}
