using System.Reflection;
using AspectCore.Abstractions.Extensions;

namespace AspectCore.Abstractions.Internal
{
    public class NonAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 3;

        public virtual bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            var declaringType = method.DeclaringType.GetTypeInfo();
            if (method.IsNonAspect() || declaringType.IsNonAspect())
            {
                return false;
            }
            return next(method);
        }
    }
}
