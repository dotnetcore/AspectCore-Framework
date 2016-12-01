using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInjectedPropertyMatcher
    {
        PropertyInfo[] Match(IInterceptor interceptor);
    }
}
