using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptorMatcher
    {
        IInterceptor[] Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo);
     }
}
