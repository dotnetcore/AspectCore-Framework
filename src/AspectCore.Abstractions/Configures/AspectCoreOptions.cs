using System.Collections.Generic;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public sealed class AspectCoreOptions
    {
        public ICollection<IInterceptorFactory> InterceptorFactories { get; } = new List<IInterceptorFactory>();

        public ICollection<NonAspectOptions> NonAspectOptions { get; } = new List<NonAspectOptions>();
    }
}