using System.Reflection;

namespace AspectCore.Abstractions.Internal.ValidationHandler
{
    public class AccessibleAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 3;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            var declaringType = method.DeclaringType.GetTypeInfo();
            if (!IsAccessibility(declaringType) || !IsAccessibility(method))
            {
                return false;
            }
            return next(method);
        }

        private bool IsAccessibility(TypeInfo declaringType)
        {
            return !(declaringType.IsNotPublic || declaringType.IsValueType || declaringType.IsSealed || !declaringType.IsNestedPublic);
        }

        private bool IsAccessibility(MethodInfo method)
        {
            return !method.IsStatic && !method.IsFinal && method.IsVirtual && (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }
    }
}