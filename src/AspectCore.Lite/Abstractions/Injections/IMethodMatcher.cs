using System.Reflection;

namespace AspectCore.Lite.Abstractions.Injections
{
    public interface IMethodMatcher
    {
        MethodInfo[] Match(IInterceptor interceptor);
    }
}
