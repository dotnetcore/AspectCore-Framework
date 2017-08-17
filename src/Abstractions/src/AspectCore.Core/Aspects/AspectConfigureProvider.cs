using AspectCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AspectConfigureProvider : IAspectConfigureProvider
    {
        private readonly static AspectConfigureProvider instance = new AspectConfigureProvider();
        public static IAspectConfigureProvider Instance
        {
            get { return instance; }
        }

        private readonly List<IInterceptorFactory> _interceptorFactories;
        private readonly List<Func<MethodInfo, bool>> _nonAspectPredicates;
        private readonly List<IAspectValidationHandler> _aspectValidationHandlers;
        private readonly IAspectConfigure _aspectConfigure;

        public IAspectConfigure AspectConfigure { get { return _aspectConfigure; } }

        private AspectConfigureProvider()
        {
            _interceptorFactories = new List<IInterceptorFactory>();
            _nonAspectPredicates = new List<Func<MethodInfo, bool>>();
            _aspectValidationHandlers = new List<IAspectValidationHandler>();
            _aspectConfigure = new AspectConfigure(_interceptorFactories, _nonAspectPredicates, _aspectValidationHandlers);
        }

        public static void AddInterceptorFactories(IEnumerable<IInterceptorFactory> interceptorFactories)
        {
            instance._interceptorFactories.AddRange(interceptorFactories);
        }

        public static void AddNonAspectPredicates(IEnumerable<Func<MethodInfo, bool>> nonAspectPredicates)
        {
            instance._nonAspectPredicates.AddRange(nonAspectPredicates);
        }

        public static void AddValidationHandlers(IEnumerable<IAspectValidationHandler> aspectValidationHandlers)
        {
            instance._aspectValidationHandlers.AddRange(aspectValidationHandlers);
        }
    }
}