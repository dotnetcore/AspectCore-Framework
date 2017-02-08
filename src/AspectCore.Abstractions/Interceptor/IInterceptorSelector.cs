using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorSelector
    {
        IInterceptor[] Select(MethodInfo method);
    }
}
