using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    public sealed class MethodInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method, TypeInfo typeInfo)
        {
            if (method.IsPropertyBinding())
            {
                return EmptyArray<IInterceptor>.Value;
            }
            return method.GetCustomAttributes().OfType<IInterceptor>();
        }
    }
}
