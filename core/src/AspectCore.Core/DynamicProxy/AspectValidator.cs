using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    [NonAspect]
    public sealed class AspectValidator : IAspectValidator
    {
        private readonly AspectValidationDelegate _aspectValidationDelegate;

        public AspectValidator(AspectValidationDelegate aspectValidationDelegate)
        {
            _aspectValidationDelegate = aspectValidationDelegate;
        }

        public bool Validate(MethodInfo method, bool isStrictValidation)
        {
            if (method == null)
            {
                return false;
            }

            var context = new AspectValidationContext { Method = method, StrictValidation = isStrictValidation };
            if (_aspectValidationDelegate(context))
            {
                return true;
            }

            var declaringTypeInfo = method.DeclaringType.GetTypeInfo();
            if (!declaringTypeInfo.IsClass)
            {
                return false;
            }

            foreach (var interfaceTypeInfo in declaringTypeInfo.GetInterfaces().Select(x => x.GetTypeInfo()))
            {
                var interfaceMethod = interfaceTypeInfo.GetMethodBySignature(new MethodSignature(method));
                if (interfaceMethod != null)
                {
                    if (Validate(interfaceMethod, isStrictValidation))
                    {
                        return true;
                    }
                }
            }

            var baseType = declaringTypeInfo.BaseType;
            if (baseType == typeof(object) || baseType == null)
            {
                return false;
            }

            var baseMethod = baseType.GetTypeInfo().GetMethodBySignature(new MethodSignature(method));
            return baseMethod != null && Validate(baseMethod, isStrictValidation);
        }
    }
}