using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorProvider
    {
        IInterceptor[] GetInterceptors(MethodInfo method);
    }
}
