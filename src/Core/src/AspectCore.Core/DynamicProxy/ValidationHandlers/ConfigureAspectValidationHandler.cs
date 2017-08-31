using System.Linq;
using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Utils;
using AspectCore.Extensions.Reflection;

namespace AspectCore.Core.DynamicProxy
{
    [NonAspect]
    public sealed class ConfigureAspectValidationHandler : IAspectValidationHandler
    {
        private readonly IAspectConfigureProvider _aspectConfigureProvider;

        public ConfigureAspectValidationHandler(IAspectConfigureProvider aspectConfigureProvider)
        {
            _aspectConfigureProvider = aspectConfigureProvider;
        }

        public int Order { get; } = 11;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            if (_aspectConfigureProvider.AspectConfigure.InterceptorFactories.Any(x => x.CanCreated(method)))
            {
                return true;
            }
            if (_aspectConfigureProvider.AspectConfigure.NonAspectPredicates.Any(x => x(method)))
            {
                return false;
            }

            return next(method);
        }
    }
}
