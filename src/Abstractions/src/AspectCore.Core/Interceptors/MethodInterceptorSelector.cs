using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Utils;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class MethodInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            if (method.IsPropertyBinding())
            {
                return ArrayUtils.Empty<IInterceptor>();
            }
            return method.GetReflector().GetCustomAttributes().OfType<IInterceptor>();
        }
    }
}
