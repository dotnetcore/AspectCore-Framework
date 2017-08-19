using System;
using System.Reflection;

namespace AspectCore.Abstractions
{
    [NonAspect]
    public interface IAspectValidator
    {
        bool Validate(MethodInfo method);
    }

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
            foreach (var method in typeInfo.DeclaredMethods)
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
            if (baseType == typeof(object))
            {
                return false;
            }
            return aspectValidator.Validate(baseType);
        }
    }
}