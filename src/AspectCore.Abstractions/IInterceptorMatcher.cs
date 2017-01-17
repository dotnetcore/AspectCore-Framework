using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorMatcher
    {
        IInterceptor[] Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo);
     }
}
