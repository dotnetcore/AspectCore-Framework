using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectConfigure
    {
        IEnumerable<IInterceptorFactory> InterceptorFactories { get; }

        IEnumerable<Func<MethodInfo, bool>> NonAspectPredicates { get; }

        IEnumerable<IAspectValidationHandler> AspectValidationHandlers { get; }
    }
}