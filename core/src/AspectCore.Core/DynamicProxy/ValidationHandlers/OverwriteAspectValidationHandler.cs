using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    public sealed class OverwriteAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 1;

        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            var method = context.Method;
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
                if (context.StrictValidation)
                {
                    return false;
                }
                if (!method.Attributes.HasFlag(MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final))
                    return false;
            }

            return next(context);
        }
    }
}