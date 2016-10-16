using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using AspectCore.Lite.Extensions;

namespace AspectCore.Lite.Internal
{
    internal class AttributeInterceptorMatcher : IInterceptorMatcher
    {
        public IInterceptor[] Match(MethodInfo method)
        {
            ExceptionUtilities.ThrowArgumentNull(method , nameof(method));
            var interceptorAttributes = method.DeclaringType.GetTypeInfo().GetCustomAttributes().Concat(method.GetCustomAttributes());
            return interceptorAttributes.OfType<IInterceptor>().Distinct(i => i.GetType()).OrderBy(i => i.Order).ToArray();
        }

        private static IEnumerable<IInterceptor> InterceptorsIterator(MethodInfo methodInfo , TypeInfo typeInfo)
        {
            foreach (var attribute in typeInfo.GetCustomAttributes())
            {
                var interceptor = attribute as IInterceptor;
                if (interceptor != null) yield return interceptor;
            }
            foreach (var attribute in methodInfo.GetCustomAttributes())
            {
                var interceptor = attribute as IInterceptor;
                if (interceptor != null) yield return interceptor;
            }
        }
    }
}
