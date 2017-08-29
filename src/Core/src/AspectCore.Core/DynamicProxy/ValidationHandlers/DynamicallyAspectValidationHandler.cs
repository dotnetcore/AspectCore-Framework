using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Core.Utils;

namespace AspectCore.Core.DynamicProxy
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
