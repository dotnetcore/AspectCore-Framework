using System.Reflection;

namespace AspectCore.Lite.Abstractions.Injections
{
    public interface IMethodSelector
    {
        MethodInfo[] Match(IInterceptor interceptor);
    }
}
