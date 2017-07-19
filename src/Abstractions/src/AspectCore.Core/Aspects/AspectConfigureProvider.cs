using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Internal;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectConfigureProvider : IAspectConfigureProvider
    {
        public IAspectConfigure AspectConfigure { get; }

        public AspectConfigureProvider(IEnumerable<IInterceptorFactory> interceptorFactories, IEnumerable<Func<MethodInfo, bool>> nonAspectPredicates)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            if (nonAspectPredicates == null)
            {
                throw new ArgumentNullException(nameof(nonAspectPredicates));
            }
            AspectConfigure = new AspectConfigure(interceptorFactories, nonAspectPredicates);
        }
    }
}
