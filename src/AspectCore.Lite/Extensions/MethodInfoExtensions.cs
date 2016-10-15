using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectCore.Lite.Extensions
{
    internal static class MethodInfoExtensions
    {
        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var returnType = methodInfo.ReturnType;

            return typeof(Task).GetTypeInfo().IsAssignableFrom(returnType);
        }

        internal static IInterceptor[] GetInterceptors(this MethodInfo methodInfo)
        {
            var interceptorAttributes = methodInfo.DeclaringType.GetTypeInfo().GetCustomAttributes().Concat(methodInfo.GetCustomAttributes());
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