using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public interface IPropertySelector
    {
        PropertyInfo[] Match(IInterceptor interceptor);
    }
}
