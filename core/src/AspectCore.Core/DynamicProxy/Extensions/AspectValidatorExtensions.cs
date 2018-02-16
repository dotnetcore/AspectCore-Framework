using System;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    public static class AspectValidatorExtensions
    {
        public static bool Validate(this IAspectValidator aspectValidator, Type type, bool isStrictValidation)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            if (type == null)
            {
                return false;
            }
            var typeInfo = type.GetTypeInfo();


            if (typeInfo.IsNonAspect()
                || typeInfo.IsProxyType()
                || typeInfo.IsValueType
                || typeInfo.IsEnum
                || !typeInfo.IsVisible())
            {
                return false;
            }

            if (typeInfo.IsClass && !typeInfo.CanInherited())
            {
                return false;
            }

            foreach (var method in typeInfo.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (aspectValidator.Validate(method, isStrictValidation))
                {
                    return true;
                }
            }

            foreach (var interfaceType in typeInfo.GetInterfaces())
            {
                if (aspectValidator.Validate(interfaceType, isStrictValidation))
                {
                    return true;
                }
            }

            var baseType = typeInfo.BaseType;

            if (baseType == null || baseType == typeof(object))
            {
                return false;
            }

            return aspectValidator.Validate(baseType, isStrictValidation);
        }
    }
}