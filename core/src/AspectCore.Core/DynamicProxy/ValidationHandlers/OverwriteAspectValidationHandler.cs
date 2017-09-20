using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    public sealed class OverwriteAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 1;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            var declaringType = method.DeclaringType.GetTypeInfo();
            if (method.IsNonAspect() || !method.IsVisibleAndVirtual())
            {
                return false;
            }
            if (!declaringType.CanInherited())
            {
                return false;
            }
            return next(method);
        }
    }
}