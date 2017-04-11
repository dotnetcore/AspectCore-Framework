using System;
using System.Linq;
using System.Reflection;

namespace AspectCore.Abstractions.Internal
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

        public static bool Validate(this IAspectValidator aspectValidator, PropertyInfo property)
        {
            if (aspectValidator == null)
            {
                throw new ArgumentNullException(nameof(aspectValidator));
            }
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            return (property.CanRead && aspectValidator.Validate(property.GetMethod)) || (property.CanWrite && aspectValidator.Validate(property.SetMethod));
        }
    }
}
