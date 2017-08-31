using AspectCore.Abstractions;
using System;
using System.Collections.Generic;
using System.Reflection;
using AspectCore.Core.Configuration;

namespace AspectCore.Core.DynamicProxy
{
    [NonAspect]
    public sealed class AspectConfigureProvider : IAspectConfigureProvider
    {
        private readonly static AspectConfigureProvider instance;
        public static IAspectConfigureProvider Instance
        {
            get { return instance; }
        }

        private readonly List<IInterceptorFactory> _interceptorFactories;
        private readonly List<Func<MethodInfo, bool>> _nonAspectPredicates;
        private readonly List<IAspectValidationHandler> _aspectValidationHandlers;
        private readonly IAspectConfigure _aspectConfigure;

        public IAspectConfigure AspectConfigure { get { return _aspectConfigure; } }

        static AspectConfigureProvider()
        {
            instance = new AspectConfigureProvider();
            instance._nonAspectPredicates.AddDefault();
            instance._aspectValidationHandlers.AddDefault();
        }

        private AspectConfigureProvider()
        {
            _interceptorFactories = new List<IInterceptorFactory>();
            _nonAspectPredicates = new List<Func<MethodInfo, bool>>();
            _aspectValidationHandlers = new List<IAspectValidationHandler>();
            _aspectConfigure = new AspectConfigure(_interceptorFactories, _nonAspectPredicates, _aspectValidationHandlers);
        }

        public static void AddInterceptorFactories(IEnumerable<IInterceptorFactory> interceptorFactories)
        {
            foreach(var item in interceptorFactories)
            {
                if (!instance._interceptorFactories.Contains(item))
                {
                    instance._interceptorFactories.Add(item);
                }
            }
        }

        public static void AddNonAspectPredicates(IEnumerable<Func<MethodInfo, bool>> nonAspectPredicates)
        {
            foreach (var item in nonAspectPredicates)
            {
                if (!instance._nonAspectPredicates.Contains(item))
                {
                    instance._nonAspectPredicates.Add(item);
                }
            }
        }

        public static void AddValidationHandlers(IEnumerable<IAspectValidationHandler> aspectValidationHandlers)
        {
            foreach (var item in aspectValidationHandlers)
            {
                if (!instance._aspectValidationHandlers.Contains(item))
                {
                    instance._aspectValidationHandlers.Add(item);
                }
            }
        }
    }
}