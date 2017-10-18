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
            if (!declaringType.CanInherited())
            {
                return false;
            }
            if (method.IsNonAspect())
            {
                return false;
            }
            if (!method.IsVisibleAndVirtual())
            {
                if (!method.Attributes.HasFlag(MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final))
                    return false;
            }

            return next(method);
        }
    }
}