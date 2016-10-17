namespace AspectCore.Lite.Abstractions
{
    public interface IPropertyInjector
    {
        IInjectedPropertyMatcher InjectedPropertyMatcher { get; }
        void Injection(IInterceptor interceptor);
    }
}
