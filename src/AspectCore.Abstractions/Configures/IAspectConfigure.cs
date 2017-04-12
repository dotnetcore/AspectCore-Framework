using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectConfigure
    {
        IEnumerable<IInterceptorFactory> InterceptorFactories { get; }

        IEnumerable<NonAspectOptions> NonAspectOptions { get; }
    }
}