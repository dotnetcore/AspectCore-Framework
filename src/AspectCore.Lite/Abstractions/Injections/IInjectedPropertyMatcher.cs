using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public interface IInjectedPropertyMatcher
    {
        PropertyInfo[] Match(IInterceptor interceptor);
    }
}
