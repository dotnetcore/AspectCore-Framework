using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Internal
{
    public sealed class InterceptorMatcher : IInterceptorMatcher
    {
        private readonly IAspectConfigure aspectConfigure;

        public InterceptorMatcher(IAspectConfigure aspectConfigure)
        {
            if (aspectConfigure == null)
            {
                throw new ArgumentNullException(nameof(aspectConfigure));
            }
            this.aspectConfigure = aspectConfigure;
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

            var aggregate = Aggregate<IInterceptor>(serviceMethod, serviceTypeInfo, aspectConfigure.GetConfigureOption<IInterceptor>());
            return aggregate.FilterMultiple().OrderBy(interceptor => interceptor.Order).ToArray();
        }

        public static IEnumerable<TInterceptor> Aggregate<TInterceptor>(
           MethodInfo methodInfo, TypeInfo typeInfo, IAspectConfigureOption<IInterceptor> configureOption)
            where TInterceptor : class, IInterceptor
        {
            foreach (var option in configureOption)
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