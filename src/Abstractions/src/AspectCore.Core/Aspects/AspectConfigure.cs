using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    [NonAspect]
    internal sealed class AspectConfigure : IAspectConfigure
    {
        public IEnumerable<IInterceptorFactory> InterceptorFactories { get; }

        public IEnumerable<Func<MethodInfo, bool>> NonAspectPredicates { get; }

        public AspectConfigure(IEnumerable<IInterceptorFactory> interceptorFactories, IEnumerable<Func<MethodInfo, bool>> nonAspectPredicates)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            if (nonAspectPredicates == null)
            {
                throw new ArgumentNullException(nameof(nonAspectPredicates));
            }

            InterceptorFactories = interceptorFactories;
            NonAspectPredicates = nonAspectPredicates;
        }
    }
}