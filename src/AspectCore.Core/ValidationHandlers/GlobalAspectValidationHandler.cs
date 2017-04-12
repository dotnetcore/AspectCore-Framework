using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Internal
{
    public class GlobalAspectValidationHandler : IAspectValidationHandler
    {
        private readonly IAspectConfigureProvider _aspectConfigureProvider;

        public GlobalAspectValidationHandler(IAspectConfigureProvider aspectConfigureProvider)
        {
            _aspectConfigureProvider = aspectConfigureProvider;
        }

        public int Order { get; } = 13;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            if (!method.IsPropertyBinding())
            {
                if (_aspectConfigureProvider.AspectConfigure.InterceptorFactories.Any(x => x.Predicate(method)))
                {
                    return true;
                }
            }

            return next(method);
        }
    }
}
