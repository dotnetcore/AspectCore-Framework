using System.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
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