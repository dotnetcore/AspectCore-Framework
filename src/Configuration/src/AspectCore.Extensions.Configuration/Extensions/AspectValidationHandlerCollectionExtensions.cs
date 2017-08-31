using System.Collections.Generic;
using AspectCore.Abstractions;
using AspectCore.Core;

namespace AspectCore.Extensions.Configuration
{
    public static class AspectValidationHandlerCollectionExtensions
    {
        internal static void AddDefault(this ICollection<IAspectValidationHandler> aspectValidationHandlers)
        {
            aspectValidationHandlers.Add(new AccessibleAspectValidationHandler());
            aspectValidationHandlers.Add(new AttributeAspectValidationHandler());
            aspectValidationHandlers.Add(new CacheAspectValidationHandler());
            aspectValidationHandlers.Add(new ConfigureAspectValidationHandler(AspectConfigureProvider.Instance));
            aspectValidationHandlers.Add(new DynamicallyAspectValidationHandler());
            aspectValidationHandlers.Add(new NonAspectValidationHandler());
        }
    }
}