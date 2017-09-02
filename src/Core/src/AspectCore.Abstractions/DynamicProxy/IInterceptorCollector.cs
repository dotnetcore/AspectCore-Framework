using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public interface IInterceptorCollector
    {
        IEnumerable<IInterceptor> Collect(MethodInfo method);
    }
}
