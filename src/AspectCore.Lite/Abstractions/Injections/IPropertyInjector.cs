namespace AspectCore.Lite.Abstractions
{
    public interface IPropertyInjector
    {
        void Injection(IInterceptor interceptor);
    }
}
