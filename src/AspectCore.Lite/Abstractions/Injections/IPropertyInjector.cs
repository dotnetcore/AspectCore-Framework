namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IPropertyInjector
    {
        void Injection(IInterceptor interceptor);
    }
}
