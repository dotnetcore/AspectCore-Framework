using AspectCore.Lite.Abstractions.Resolution.Common;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class InterceptorMatcher : IInterceptorMatcher
    {
        private static readonly ConcurrentDictionary<MethodInfo, IInterceptor[]> InterceptorCache = new ConcurrentDictionary<MethodInfo, IInterceptor[]>();

        private readonly IAspectConfiguration aspectConfiguration;

        public InterceptorMatcher(IAspectConfiguration aspectConfiguration)
        {
            this.aspectConfiguration = aspectConfiguration;
        }

        public IInterceptor[] Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo)
        {
            return InterceptorCache.GetOrAdd(serviceMethod, _ =>
            {
                var aggregate = Aggregate(serviceMethod, serviceTypeInfo, aspectConfiguration.GetConfigurationOption<IInterceptor>());
                return aggregate.DuplicateRemoval().OrderBy(interceptor => interceptor.Order).ToArray();
            });
        }

        private IEnumerable<IInterceptor> Aggregate(
           MethodInfo methodInfo, TypeInfo typeInfo, IConfigurationOption<IInterceptor> configurationOption)
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

            foreach (var option in configurationOption)
            {
                var interceptor = option(methodInfo);
                if (interceptor != null) yield return interceptor;
            }
        }
    }
}
