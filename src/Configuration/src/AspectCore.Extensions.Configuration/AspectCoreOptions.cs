using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core;

namespace AspectCore.Extensions.Configuration
{
    public class AspectCoreOptions
    {
        public ICollection<IInterceptorFactory> InterceptorFactories { get; } 

        public ICollection<Func<MethodInfo, bool>> NonAspectPredicates { get; } 

        public ICollection<IAspectValidationHandler> AspectValidationHandlers { get; }

        public IServiceContainer InternalServices { get; }

        public AspectCoreOptions(IServiceContainer services)
        {
            InternalServices = services ?? new ServiceContainer();
            AspectValidationHandlers = new List<IAspectValidationHandler>();
            InterceptorFactories = new List<IInterceptorFactory>();
            NonAspectPredicates= new List<Func<MethodInfo, bool>>();
            AspectValidationHandlers.AddDefault();
            NonAspectPredicates.AddDefault();
        }
    }
}