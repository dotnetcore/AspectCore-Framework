using System.Linq;
using System.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AttributeAspectValidationHandler : IAspectValidationHandler
    {
        public int Order { get; } = 13;

        public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
        {
            var declaringType = context.Method.DeclaringType.GetTypeInfo();

            if (IsAttributeAspect(declaringType) || IsAttributeAspect(context.Method))
            {
                return true;
            }

            return next(context);
        }

        private bool IsAttributeAspect(MemberInfo member)
        {
            return member.CustomAttributes.Any(data => typeof(IInterceptor).GetTypeInfo().IsAssignableFrom(data.AttributeType));
        }
    }
}