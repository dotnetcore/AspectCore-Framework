using System;
using System.Collections.Generic;

namespace AspectCore.Lite.Abstractions.Resolution.Extensions
{
    internal static class InterceptorMatcherExtensions
    {
        internal static IEnumerable<IInterceptor> DuplicateRemoval(this IEnumerable<IInterceptor> source)
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
