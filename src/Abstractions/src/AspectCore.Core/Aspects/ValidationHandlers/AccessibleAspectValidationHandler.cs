using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Utils;

namespace AspectCore.Core
{
    [NonAspect]
    public sealed class AccessibleAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 5;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            var declaringType = method.DeclaringType.GetTypeInfo();
            if (!declaringType.IsAccessibility() || !method.IsAccessibility())
            {
                return false;
            }
            return next(method);
        }
    }
}