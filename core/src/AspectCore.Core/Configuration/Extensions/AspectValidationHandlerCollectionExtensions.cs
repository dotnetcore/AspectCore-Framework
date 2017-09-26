using AspectCore.DynamicProxy;

namespace AspectCore.Configuration
{
    internal static class AspectValidationHandlerCollectionExtensions
    {
        internal static AspectValidationHandlerCollection AddDefault(this AspectValidationHandlerCollection aspectValidationHandlers, IAspectConfiguration configuration)
        {
            aspectValidationHandlers.Add(new OverwriteAspectValidationHandler());
            aspectValidationHandlers.Add(new AttributeAspectValidationHandler());
            aspectValidationHandlers.Add(new CacheAspectValidationHandler());
            aspectValidationHandlers.Add(new ConfigureAspectValidationHandler(configuration));
            return aspectValidationHandlers;
        }
    }
}