namespace AspectCore.Lite.Abstractions
{
    public interface IMethodInjector
    {
        void Injection(IInterceptor interceptor);
    }
}
