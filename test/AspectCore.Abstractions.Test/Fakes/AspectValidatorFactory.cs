using System.Collections.Generic;
using AspectCore.Abstractions.Internal;

namespace AspectCore.Abstractions.Test.Fakes
{
    public class AspectValidatorFactory
    {
        public static IAspectValidator GetAspectValidator(IAspectConfigure configure)
        {
            var handlers = new List<IAspectValidationHandler>
            {
               new AccessibleAspectValidationHandler(),
               new AttributeAspectValidationHandler(),
               new CacheAspectValidationHandler(),
               new ConfigureAspectValidationHandler(configure),
               new DynamicallyAspectValidationHandler(),
               new IgnoreAspectValidationHandler(configure),
               new NonAspectValidationHandler()
            };
            var aspectValidatorBuilder = new AspectValidatorBuilder(handlers);
            return aspectValidatorBuilder.Build();
        }
    }
}