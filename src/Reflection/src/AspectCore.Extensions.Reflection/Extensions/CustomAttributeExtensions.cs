using System;
using System.Collections.Generic;
using System.Linq;

namespace AspectCore.Extensions.Reflection
{
    public static class CustomAttributeExtensions
    {
        public static Attribute[] GetCustomAttributes(this ICustomAttributeReflectorProvider customAttributeReflectorProvider)
        {
            if (customAttributeReflectorProvider == null)
            {
                throw new ArgumentNullException(nameof(customAttributeReflectorProvider));
            }
            var customAttributeReflectors = customAttributeReflectorProvider.CustomAttributeReflectors;
            var customAttributeLength = customAttributeReflectors.Length;
            var attributes = new Attribute[customAttributeLength];
            for (var i = 0; i < customAttributeLength; i++)
            {
                attributes[i] = customAttributeReflectors[i].Invoke();
            }
            return attributes;
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this ICustomAttributeReflectorProvider customAttributeReflectorProvider, Type attributeType)
        {
            if (customAttributeReflectorProvider == null)
            {
                throw new ArgumentNullException(nameof(customAttributeReflectorProvider));
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException(nameof(attributeType));
            }
            return customAttributeReflectorProvider.CustomAttributeReflectors.
                Where(reflector => reflector.AttributeType == attributeType).
                Select(reflector => (Attribute)reflector.Invoke());
        }

        public static IEnumerable<TAttribute> GetCustomAttributes<TAttribute>(this ICustomAttributeReflectorProvider customAttributeReflectorProvider)
            where TAttribute : Attribute
        {
            return GetCustomAttributes(customAttributeReflectorProvider, typeof(TAttribute)).OfType<TAttribute>();
        }

        public static Attribute GetCustomAttribute(this ICustomAttributeReflectorProvider customAttributeReflectorProvider, Type attributeType)
        {
            if (customAttributeReflectorProvider == null)
            {
                throw new ArgumentNullException(nameof(customAttributeReflectorProvider));
            }
            var customAttributeReflectors = customAttributeReflectorProvider.CustomAttributeReflectors;
            var customAttributeLength = customAttributeReflectors.Length;
            for (var i = 0; i < customAttributeLength; i++)
            {
                if (customAttributeReflectors[i].AttributeType == attributeType)
                {
                    return customAttributeReflectors[i].Invoke();
                }
            }
            return null;
        }

        public static TAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeReflectorProvider customAttributeReflectorProvider)
           where TAttribute : Attribute
        {
            return GetCustomAttributes<TAttribute>(customAttributeReflectorProvider).FirstOrDefault();
        }
    }
}
