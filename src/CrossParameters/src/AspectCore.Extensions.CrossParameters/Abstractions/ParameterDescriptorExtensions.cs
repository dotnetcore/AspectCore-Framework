using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Extensions.CrossParameters
{
    internal static class ParameterDescriptorExtensions
    {
        private readonly static ConcurrentDictionary<ParameterInfo, IParameterInterceptor[]> parameterInterceptorCahce
            = new ConcurrentDictionary<ParameterInfo, IParameterInterceptor[]>();

        public static IParameterInterceptor[] MatchInterceptors(this IParameterDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            return parameterInterceptorCahce.GetOrAdd(descriptor.ParameterInfo, _ => descriptor.GetCustomAttributes(false).OfType<IParameterInterceptor>().ToArray());
        }
    }
}
