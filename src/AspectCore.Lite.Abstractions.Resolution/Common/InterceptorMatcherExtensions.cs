using System;
using System.Collections.Generic;

namespace AspectCore.Lite.Abstractions.Resolution.Common
{
    internal static class InterceptorMatcherExtensions
    {
        public static IEnumerable<IInterceptor> DuplicateRemoval(this IEnumerable<IInterceptor> source)
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
