using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Internal
{
    public class IgnoreAspectValidationHandler : IAspectValidationHandler
    {
        private readonly IAspectConfigure _aspectConfigure;

        public IgnoreAspectValidationHandler(IAspectConfigure aspectConfigure)
        {
            _aspectConfigure = aspectConfigure;
        }

        public int Order { get; } = 11;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            if (_aspectConfigure.GetConfigureOption<bool>().Any(configure => configure(method)))
            {
                return false;
            }
            return next(method);
        }
    }
}
