using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    public interface IInjectedMethodMatcher
    {
        MethodInfo[] Match(IInterceptor interceptor);
    }
}
