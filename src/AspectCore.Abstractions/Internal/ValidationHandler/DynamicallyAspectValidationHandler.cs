using System.Reflection;
using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Internal
{
    public class DynamicallyAspectValidationHandler : IAspectValidationHandler
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
