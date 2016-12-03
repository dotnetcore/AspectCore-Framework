using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Lite.Abstractions;

namespace AspectCore.Lite.Internal
{
    internal sealed class DefaultInterceptorMatcher : IInterceptorMatcher
    {
        private readonly IInterceptorTable interceptorTable;

        public DefaultInterceptorMatcher(IInterceptorTable interceptorTable)
        {
            this.interceptorTable = interceptorTable;
        }

        public IEnumerable<IInterceptor> Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo)
        {
            return MultipleInterceptorIterator(AllInterceptorIterator(serviceMethod, serviceTypeInfo, interceptorTable)).OrderBy(interceptor => interceptor.Order);
        }

        private static IEnumerable<IInterceptor> AllInterceptorIterator(
           MethodInfo methodInfo, TypeInfo typeInfo, IEnumerable<IInterceptor> interceptorCollection)
        {
            foreach (var attribute in methodInfo.GetCustomAttributes())
            {
                var interceptor = attribute as IInterceptor;
                if (interceptor != null) yield return interceptor;
            }

            foreach (var attribute in typeInfo.GetCustomAttributes())
            {
                var interceptor = attribute as IInterceptor;
                if (interceptor != null) yield return interceptor;
            }

            foreach (var interceptor in interceptorCollection)
            {
                yield return interceptor;
            }
        }

        private static IEnumerable<IInterceptor> MultipleInterceptorIterator(IEnumerable<IInterceptor> interceptors)
        {
            var existed = new HashSet<Type>();

            foreach (var interceptor in interceptors)
            {
                if (interceptor.AllowMultiple)
                {
                    yield return interceptor;
                    continue;
                }
                if (existed.Add(interceptor.GetType()))
                {
                    yield return interceptor;
                }
            }
        }
    }
}
