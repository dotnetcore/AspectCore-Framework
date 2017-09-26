using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IInterceptorCollector
    {
        IEnumerable<IInterceptor> Collect(MethodInfo serviceMethod, MethodInfo implementationMethod);
    }
}
