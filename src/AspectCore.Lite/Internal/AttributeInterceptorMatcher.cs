using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using AspectCore.Lite.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal class AttributeInterceptorMatcher : IInterceptorMatcher
    {
        private static IEnumerable<IInterceptor> InterceptorsIterator(MethodInfo methodInfo, TypeInfo typeInfo)
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

        public IInterceptor[] Match(MethodInfo method, TypeInfo typeInfo)
        {
            ExceptionHelper.ThrowArgumentNull(method, nameof(method));
            ExceptionHelper.ThrowArgumentNull(typeInfo, nameof(typeInfo));
            var interceptorAttributes = InterceptorsIterator(method, typeInfo);
            return interceptorAttributes.Distinct(i => i.GetType()).OrderBy(i => i.Order).ToArray();
        }
    }
}
