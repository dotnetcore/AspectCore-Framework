using System;
using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;

namespace AspectCore.Core.DynamicProxy
{
    [NonAspect]
    public sealed class ConfigureAspectValidationHandler : IAspectValidationHandler
    {
        private readonly IAspectConfiguration _aspectConfiguration;

        public ConfigureAspectValidationHandler(IAspectConfiguration aspectConfiguration)
        {
            _aspectConfiguration = aspectConfiguration ?? throw new ArgumentNullException(nameof(aspectConfiguration));
        }

        public int Order { get; } = 11;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            if (_aspectConfiguration.Interceptors.Any(x => x.CanCreated(method)))
            {
                return true;
            }
            if (_aspectConfiguration.NonAspectPredicates.Any(x => x(method)))
            {
                return false;
            }

            return next(method);
        }
    }
}