using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Extensions
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
                throw new ArgumentNullException(nameof(type));
            }

            return Validate(aspectValidator, type.GetTypeInfo());
        }

        public static bool Validate(this IAspectValidator aspectValidator, TypeInfo typeInfo)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (typeInfo.IsValueType)
            {
                return false;
            }

            return typeInfo.DeclaredMethods.Any(method => aspectValidator.Validate(method));
        }
    }
}
