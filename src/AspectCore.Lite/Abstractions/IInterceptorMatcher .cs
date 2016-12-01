using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions
{
    [NonAspect]
    public interface IInterceptorMatcher 
    {
        IInterceptor[] Match(MethodInfo method, TypeInfo typeInfo);
    }
}
