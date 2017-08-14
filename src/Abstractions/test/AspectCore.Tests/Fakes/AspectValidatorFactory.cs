using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AspectCore.Abstractions;
using AspectCore.Core;

namespace AspectCore.Tests.Fakes
{
    public class AspectValidatorFactory
    {
        public static IAspectValidatorBuilder Create()
        {
            var aspectConfigureProvider = AspectConfigureProvider.Instance;
            var handlers = new List<IAspectValidationHandler>();
            handlers.Add(new AccessibleAspectValidationHandler());
            handlers.Add(new AttributeAspectValidationHandler());
            handlers.Add(new CacheAspectValidationHandler());
            handlers.Add(new ConfigureAspectValidationHandler(aspectConfigureProvider));
            handlers.Add(new DynamicallyAspectValidationHandler());
            handlers.Add(new NonAspectValidationHandler());

            AspectConfigureProvider.AddValidationHandlers(handlers);

            var builder = new AspectValidatorBuilder(aspectConfigureProvider);
            return builder;
        }
    }
}
