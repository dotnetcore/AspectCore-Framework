using System;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;

namespace AspectCore.DynamicProxy
{
    public sealed class ConfigureAspectValidationHandler : IAspectValidationHandler
    {
        private readonly IAspectConfiguration _aspectConfiguration;

        public ConfigureAspectValidationHandler(IAspectConfiguration aspectConfiguration)
        {
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
        }

        public int Order { get; } = 11;

        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            if (context.StrictValidation)
            {
                var method = context.Method;
                if (_aspectConfiguration.NonAspectPredicates.Any(x => x(method)))
                {
                    return false;
                }
                if (_aspectConfiguration.Interceptors.Where(x => x.Predicates.Length != 0).Any(x => x.CanCreated(method)))
                {
                    return true;
                }
                if (_aspectConfiguration.Interceptors.Where(x => x.Predicates.Length == 0).Any(x => x.CanCreated(method)))
                {
                    return true;
                }
            }
           
            return next(context);
        }
    }
}