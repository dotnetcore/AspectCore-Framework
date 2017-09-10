using System.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class DynamicallyAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 1;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            if (method.DeclaringType.GetTypeInfo().IsProxyType())
            {
                return false;
            }
            return next(method);
        }
    }
}
