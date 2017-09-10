using System.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
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
