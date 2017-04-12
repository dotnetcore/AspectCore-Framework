using System.Collections.Generic;
using AspectCore.Abstractions;
using System;

namespace AspectCore.Core
{
    public sealed class AspectConfigure : IAspectConfigure
    {
        private readonly IEnumerable<IInterceptorFactory> _interceptorFactories;
        private readonly IEnumerable<NonAspectOptions> _nonAspectOptions;

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

        public IEnumerable<IInterceptorFactory> InterceptorFactories { get; }

        public IEnumerable<NonAspectOptions> NonAspectOptions { get; }
    }
}