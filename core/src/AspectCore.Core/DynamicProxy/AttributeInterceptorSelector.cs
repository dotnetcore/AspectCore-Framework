using System.Collections.Generic;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AttributeInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            foreach(var attribute in method.DeclaringType.GetTypeInfo().GetReflector().GetCustomAttributes())
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }
            foreach (var attribute in method.GetReflector().GetCustomAttributes())
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }
        }
    }
}