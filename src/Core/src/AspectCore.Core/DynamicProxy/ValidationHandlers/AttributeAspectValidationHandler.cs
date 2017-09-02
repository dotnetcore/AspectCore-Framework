using System.Linq;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AttributeAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 13;

        public bool Invoke(MethodInfo method, AspectValidationDelegate next)
        {
            var declaringType = method.DeclaringType.GetTypeInfo();

            if (IsAttributeAspect(declaringType) || IsAttributeAspect(method))
            {
                return true;
            }

            return next(method);
        }

        private bool IsAttributeAspect(MemberInfo member)
        {
            return member.CustomAttributes.Any(data => typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(data.AttributeType.GetTypeInfo()));
        }
    }
}