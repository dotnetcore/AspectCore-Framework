using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    public interface IInterceptorSelector
    {
        IEnumerable<IInterceptor> Select(MethodInfo method);
    }
}