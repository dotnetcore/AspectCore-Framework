using System;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    public static class AspectValidatorExtensions
    {
        public static bool Validate(this IAspectValidator aspectValidator, Type type)
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

            if (typeInfo.IsValueType)
            {
                return false;
            }

            if (typeInfo.GetReflector().IsDefined<NonAspectAttribute>() || typeInfo.GetReflector().IsDefined<DynamicallyAttribute>())
            {
                return false;
            }

            if (!typeInfo.IsVisible())
            {
                return false;
            }

            foreach (var method in typeInfo.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (aspectValidator.Validate(method))
                {
                    return true;
                }
            }

            foreach (var interfaceType in typeInfo.GetInterfaces())
            {
                if (aspectValidator.Validate(interfaceType))
                {
                    return true;
                }
            }

            var baseType = typeInfo.BaseType;

            if (baseType == null || baseType == typeof(object))
            {
                return false;
            }

            return aspectValidator.Validate(baseType);
        }
    }
}