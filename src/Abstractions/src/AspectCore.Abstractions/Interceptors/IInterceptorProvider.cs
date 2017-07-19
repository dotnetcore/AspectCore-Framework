using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorProvider
    {
        IEnumerable<IInterceptor> GetInterceptors(MethodInfo method);
    }
}
