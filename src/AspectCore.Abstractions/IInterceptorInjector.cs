namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorInjector
    {
        void Inject(IInterceptor interceptor);
    }
}
