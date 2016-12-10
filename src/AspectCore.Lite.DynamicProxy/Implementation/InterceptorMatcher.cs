using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.DynamicProxy.Implementation
{
    internal sealed class InterceptorMatcher : IInterceptorMatcher
    {
        private static readonly ConcurrentDictionary<MethodInfo, IInterceptor[]> InterceptorPool = new ConcurrentDictionary<MethodInfo, IInterceptor[]>();

        private readonly IInterceptorCollection interceptorCollection;

        public InterceptorMatcher(IInterceptorCollection interceptorTable)
        {
            this.interceptorCollection = interceptorTable;
        }

        public IInterceptor[] Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo)
        {
            return InterceptorPool.GetOrAdd(serviceMethod, _ =>
                MultipleInterceptorIterator(AllInterceptorIterator(serviceMethod, serviceTypeInfo, interceptorCollection)).OrderBy(interceptor => interceptor.Order).ToArray());
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
