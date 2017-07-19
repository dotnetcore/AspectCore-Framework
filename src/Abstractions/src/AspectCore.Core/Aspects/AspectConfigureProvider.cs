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

        public AspectConfigureProvider(ICollection<IInterceptorFactory> interceptorFactories, ICollection<Func<MethodInfo, bool>> nonAspectPredicates)
        {
            if (interceptorFactories == null)
            {
                throw new ArgumentNullException(nameof(interceptorFactories));
            }
            if (nonAspectPredicates == null)
            {
                throw new ArgumentNullException(nameof(nonAspectPredicates));
            }
            nonAspectPredicates
              .AddObjectVMethod().AddSystem().AddAspNetCore().AddEntityFramework().AddOwin().AddPageGenerator();
            AspectConfigure = new AspectConfigure(interceptorFactories, nonAspectPredicates);
        }
    }
}
