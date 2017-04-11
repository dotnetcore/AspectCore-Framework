using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Internal
{
    public class ConfigureAspectValidationHandler : IAspectValidationHandler
    {

        private readonly IAspectConfigure _aspectConfigure;

        public ConfigureAspectValidationHandler(IAspectConfigure aspectConfigure)
        {
            _aspectConfigure = aspectConfigure;
        }

        public int Order { get; } = 15;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            if (IsConfigureAspect(method))
            {
                return true;
            }

            return next(method);
        }

        private bool IsConfigureAspect(MethodInfo method)
        {
            return _aspectConfigure.GetConfigureOption<IInterceptor>().Any(config => (config(method) as IInterceptor) != null);
        }
    }
}
