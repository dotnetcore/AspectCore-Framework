using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AttributeInterceptorSelector : IInterceptorSelector
    {
        public IEnumerable<IInterceptor> Select(MethodInfo method)
        {
            if (!RuntimeFeature.IsDynamicCodeSupported)
            {
                // NativeAOT: use standard reflection to avoid DynamicMethod
                foreach (var attribute in method.DeclaringType.GetTypeInfo().GetCustomAttributes(true))
                {
                    if (attribute is IInterceptor interceptor)
                        yield return interceptor;
                }
                foreach (var attribute in method.GetCustomAttributes(true))
                {
                    if (attribute is IInterceptor interceptor)
                        yield return interceptor;
                }
            }
            else
            {
                foreach (var attribute in method.DeclaringType.GetTypeInfo().GetReflector().GetCustomAttributes())
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
}