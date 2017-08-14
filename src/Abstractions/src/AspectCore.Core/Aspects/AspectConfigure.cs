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

        public IEnumerable<IAspectValidationHandler> AspectValidationHandlers { get; }

        public AspectConfigure(IEnumerable<IInterceptorFactory> interceptorFactories, IEnumerable<Func<MethodInfo, bool>> nonAspectPredicates, IEnumerable<IAspectValidationHandler> aspectValidationHandlers)
        {
            InterceptorFactories = interceptorFactories;
            NonAspectPredicates = nonAspectPredicates;
            AspectValidationHandlers = aspectValidationHandlers;
        }
    }
}