using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptorMatcher
    {
        IEnumerable<IInterceptor> Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo);
     }
}
