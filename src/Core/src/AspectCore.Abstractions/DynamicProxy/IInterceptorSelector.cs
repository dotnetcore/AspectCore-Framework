using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorSelector
    {
        IEnumerable<IInterceptor> Select(MethodInfo method);
    }
}