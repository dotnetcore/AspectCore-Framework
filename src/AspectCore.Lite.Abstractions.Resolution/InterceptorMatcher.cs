using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Resolution
{
    public sealed class InterceptorMatcher : IInterceptorMatcher
    {
        private static readonly ConcurrentDictionary<MethodInfo, IInterceptor[]> InterceptorPool = new ConcurrentDictionary<MethodInfo, IInterceptor[]>();

        private readonly IAspectConfiguration aspectConfigurator;

        public InterceptorMatcher(IAspectConfiguration aspectConfigurator)
        {
            this.aspectConfigurator = aspectConfigurator;
        }

        public IInterceptor[] Match(MethodInfo serviceMethod, TypeInfo serviceTypeInfo)
        {
            return InterceptorPool.GetOrAdd(serviceMethod, _ =>
            {
                var configuredInterceptors = ((IEnumerable<Func<MethodInfo, IInterceptor>>)aspectConfigurator).Select(factory => factory.Invoke(serviceMethod)).OfType<IInterceptor>();

                var matchInterceptors = AllInterceptorIterator(serviceMethod, serviceTypeInfo, configuredInterceptors);

                return MultipleInterceptorIterator(matchInterceptors).OrderBy(interceptor => interceptor.Order).ToArray();
            });
        }

        private static IEnumerable<IInterceptor> AllInterceptorIterator(
           MethodInfo methodInfo, TypeInfo typeInfo, IEnumerable<IInterceptor> configuredInterceptors)
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

            foreach (var interceptor in configuredInterceptors)
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
