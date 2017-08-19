using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IInterceptorCollector
    {
        IEnumerable<IInterceptor> Collect(MethodInfo method);
    }
}
