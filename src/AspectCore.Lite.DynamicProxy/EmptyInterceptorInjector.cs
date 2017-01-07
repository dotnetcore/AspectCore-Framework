using AspectCore.Lite.Abstractions;

namespace AspectCore.Lite.DynamicProxy
{
    internal class EmptyInterceptorInjector : IInterceptorInjector
    {
        public void Inject(IInterceptor interceptor)
        {
        }
    }
}
