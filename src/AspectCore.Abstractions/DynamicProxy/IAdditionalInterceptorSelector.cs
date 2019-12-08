using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public interface IAdditionalInterceptorSelector
    {
        IEnumerable<IInterceptor> Select(MethodInfo serviceMethod, MethodInfo implementationMethod);
    }
}
