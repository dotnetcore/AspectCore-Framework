using System;
using System.Collections.Concurrent;
using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Common;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal sealed class AttributeInterceptorMatcher : IInterceptorMatcher
    {
        private static readonly ConcurrentDictionary<MethodInfo, IInterceptor[]> AttributeInterceptorCache =
            new ConcurrentDictionary<MethodInfo, IInterceptor[]>();

        private readonly IInterceptorCollection interceptorCollection;

        public AttributeInterceptorMatcher(IInterceptorCollection interceptorCollection)
        {
            this.interceptorCollection = interceptorCollection;
        }

        public IInterceptor[] Match(MethodInfo method, TypeInfo typeInfo)
        {
            ExceptionHelper.ThrowArgumentNull(method, nameof(method));
            ExceptionHelper.ThrowArgumentNull(typeInfo, nameof(typeInfo));
            return AttributeInterceptorCache.GetOrAdd(method, key =>
            {
                var interceptorAttributes = InterceptorsIterator(method, typeInfo, interceptorCollection);
                return InterceptorsFilter(interceptorAttributes).OrderBy(i => i.Order).ToArray();
            });
        }

        private static IEnumerable<IInterceptor> InterceptorsIterator(
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

        private static IEnumerable<IInterceptor> InterceptorsFilter(IEnumerable<IInterceptor> interceptors)
        {
            var set = new HashSet<Type>();
            foreach (var interceptor in interceptors)
            {
                if (interceptor.AllowMultiple)
                {
                    yield return interceptor;
                }
                else
                {
                    if (set.Add(interceptor.GetType()))
                    {
                        yield return interceptor;
                    }
                }
            }
        }
    }
}
