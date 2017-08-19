using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class TypeInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            if (method.IsPropertyBinding())
            {
                return EmptyArray<IInterceptor>.Value;
            }
            return method.DeclaringType.GetReflector().GetCustomAttributes().OfType<IInterceptor>();
        }
    }
}