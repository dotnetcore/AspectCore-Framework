using System;
using System.Collections.Generic;
using AspectCore.Abstractions;

namespace AspectCore.Core
{
    internal sealed class AspectConfigure : IAspectConfigure
    {
        public IEnumerable<IInterceptorFactory> InterceptorFactories { get; }

        public IEnumerable<NonAspectOptions> NonAspectOptions { get; }

        public AspectConfigure(IEnumerable<IInterceptorFactory> interceptorFactories, IEnumerable<NonAspectOptions> nonAspectOptions)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            if (nonAspectOptions == null)
            {
                throw new ArgumentNullException(nameof(nonAspectOptions));
            }

            InterceptorFactories = interceptorFactories;
            NonAspectOptions = nonAspectOptions;
        }


    }
}