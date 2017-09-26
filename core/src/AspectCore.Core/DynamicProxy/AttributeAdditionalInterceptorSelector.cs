using System.Collections.Generic;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AttributeAdditionalInterceptorSelector : IAdditionalInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo serviceMethod, MethodInfo implementationMethod)
        {
            foreach (var attribute in implementationMethod.DeclaringType.GetTypeInfo().GetReflector().GetCustomAttributes())
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }
            foreach (var attribute in implementationMethod.GetReflector().GetCustomAttributes())
            {
                if (attribute is IInterceptor interceptor)
                    yield return interceptor;
            }
        }
    }
}
