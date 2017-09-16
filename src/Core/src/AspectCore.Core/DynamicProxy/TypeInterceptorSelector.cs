using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class TypeInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            if (method.IsPropertyBinding())
            {
                return ArrayUtils.Empty<IInterceptor>();
            }
            return method.DeclaringType.GetTypeInfo().GetReflector().GetCustomAttributes().OfType<IInterceptor>();
        }
    }
}