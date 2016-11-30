namespace AspectCore.Lite.Abstractions
{
    public interface IMethodInjector
    {
        IInjectedMethodMatcher InjectedMethodMatcher { get; }

        void Injection(IInterceptor interceptor);
    }
}
