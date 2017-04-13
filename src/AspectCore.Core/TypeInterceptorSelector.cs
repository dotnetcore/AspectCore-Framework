using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    public class TypeInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method, TypeInfo typeInfo)
        {
            if (method.IsPropertyBinding())
            {
                return EmptyArray<IInterceptor>.Value;
            }
            return typeInfo.GetCustomAttributes().OfType<IInterceptor>();
        }
    }
}
