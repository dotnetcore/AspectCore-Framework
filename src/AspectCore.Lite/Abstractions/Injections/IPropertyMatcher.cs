using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public interface IPropertyMatcher
    {
        PropertyInfo[] Match(IInterceptor interceptor);
    }
}
