using AspectCore.Abstractions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Resolution
{
    public sealed class InterceptorMatcher : IInterceptorMatcher
    {
        private static readonly IDictionary<MethodInfo, IInterceptor[]> InterceptorCache = new Dictionary<MethodInfo, IInterceptor[]>();
        private static object CacheLock = new object();

        private readonly IAspectConfiguration aspectConfiguration;

        public InterceptorMatcher(IAspectConfiguration aspectConfiguration)
        {
            if (aspectConfiguration == null)
            {
                throw new ArgumentNullException(nameof(aspectConfiguration));
            }
            this.aspectConfiguration = aspectConfiguration;
        }

        public IInterceptor[] Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo)
        {
            if (serviceMethod == null)
            {
                throw new ArgumentNullException(nameof(serviceMethod));
            }
            if (serviceTypeInfo == null)
            {
                throw new ArgumentNullException(nameof(serviceTypeInfo));
            }

            return InterceptorCache.GetOrAdd(serviceMethod, _ =>
            {
                var aggregate = Aggregate<IInterceptor>(serviceMethod, serviceTypeInfo, aspectConfiguration.GetConfigurationOption<IInterceptor>());
                return aggregate.FilterMultiple().OrderBy(interceptor => interceptor.Order).ToArray();
            }, ref CacheLock);
        }

        public static IEnumerable<TInterceptor> Aggregate<TInterceptor>(
           MethodInfo methodInfo, TypeInfo typeInfo, IConfigurationOption<IInterceptor> configurationOption)
            where TInterceptor : class, IInterceptor
        {
            foreach (var option in configurationOption)
            {
                var interceptor = option(methodInfo) as TInterceptor;
                if (interceptor != null) yield return interceptor;
            }

            foreach (var attribute in typeInfo.GetCustomAttributes())
            {
                var interceptor = attribute as TInterceptor;
                if (interceptor != null) yield return interceptor;
            }

            foreach (var attribute in methodInfo.GetCustomAttributes())
            {
                var interceptor = attribute as TInterceptor;
                if (interceptor != null) yield return interceptor;
            }
        }
    }
}
