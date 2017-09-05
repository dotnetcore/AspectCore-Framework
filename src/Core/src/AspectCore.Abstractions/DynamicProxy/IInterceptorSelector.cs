using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IInterceptorSelector
    {
        IEnumerable<IInterceptor> Select(MethodInfo method);
    }
}