using System;
using System.Collections.Generic;

namespace AspectCore.Abstractions.Internal
{
    public static class InterceptorMatcherExtensions
    {
        public static IEnumerable<T> FilterMultiple<T>(this IEnumerable<T> source)
            where T : IInterceptor
        {
            var set = new HashSet<Type>();

            foreach (var interceptor in source)
            {
                if (interceptor.AllowMultiple)
                {
                    yield return interceptor;
                    continue;
                }
                if (set.Add(interceptor.GetType()))
                {
                    yield return interceptor;
                }
            }
        }
    }
}
