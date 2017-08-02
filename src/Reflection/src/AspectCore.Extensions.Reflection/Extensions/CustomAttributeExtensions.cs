using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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
            if (customAttributeLength == 0)
            {
                return new Attribute[0];
            }
            var attrs = new Attribute[customAttributeLength];
            for (var i = 0; i < customAttributeLength; i++)
            {
                attrs[i] = customAttributeReflectors[i].Invoke();
            }
            return attrs;
        }

        public static Attribute[] GetCustomAttributes(this ICustomAttributeReflectorProvider customAttributeReflectorProvider, Type attributeType)
        {
            if (customAttributeReflectorProvider == null)
            {
                throw new ArgumentNullException(nameof(customAttributeReflectorProvider));
            }
            if (attributeType == null)
            {
                throw new ArgumentNullException(nameof(attributeType));
            }
            var customAttributeReflectors = customAttributeReflectorProvider.CustomAttributeReflectors;
            var customAttributeLength = customAttributeReflectors.Length;
            if (customAttributeLength == 0)
            {
                return new Attribute[0];
            }
            var checkedAttrs = new Attribute[customAttributeLength];
            var @checked = 0;
            var attrToken = new AttributeToken(attributeType.GetTypeInfo().MetadataToken);
            for (var i = 0; i < customAttributeLength; i++)
            {
                var reflector = customAttributeReflectors[i];
                if (reflector._tokens.Contains(attrToken))
                    checkedAttrs[@checked++] = reflector.Invoke();
            }
            var attrs = new Attribute[@checked];
            Array.Copy(checkedAttrs, attrs, @checked);
            return attrs;
        }

        public static TAttribute[] GetCustomAttributes<TAttribute>(this ICustomAttributeReflectorProvider customAttributeReflectorProvider)
            where TAttribute : Attribute
        {
            if (customAttributeReflectorProvider == null)
            {
                throw new ArgumentNullException(nameof(customAttributeReflectorProvider));
            }
            var customAttributeReflectors = customAttributeReflectorProvider.CustomAttributeReflectors;
            var customAttributeLength = customAttributeReflectors.Length;
            if (customAttributeLength == 0)
            {
                return new TAttribute[0];
            }
            var checkedAttrs = new TAttribute[customAttributeLength];
            var @checked = 0;
            var attrToken = new AttributeToken(typeof(TAttribute).GetTypeInfo().MetadataToken);
            for (var i = 0; i < customAttributeLength; i++)
            {
                var reflector = customAttributeReflectors[i];
                if (reflector._tokens.Contains(attrToken))
                    checkedAttrs[@checked++] = (TAttribute)reflector.Invoke();
            }
            var attrs = new TAttribute[@checked];
            Array.Copy(checkedAttrs, attrs, @checked);
            return attrs;
        }

        public static Attribute GetCustomAttribute(this ICustomAttributeReflectorProvider customAttributeReflectorProvider, Type attributeType)
        {
            if (customAttributeReflectorProvider == null)
            {
                throw new ArgumentNullException(nameof(customAttributeReflectorProvider));
            }
            var customAttributeReflectors = customAttributeReflectorProvider.CustomAttributeReflectors;
            var customAttributeLength = customAttributeReflectors.Length;
            if (customAttributeLength == 0)
            {
                return null;
            }
            var attrToken = new AttributeToken(attributeType.GetTypeInfo().MetadataToken);
            for (var i = 0; i < customAttributeLength; i++)
            {
                var reflector = customAttributeReflectors[i];
                if (reflector._tokens.Contains(attrToken))
                {
                    return customAttributeReflectors[i].Invoke();
                }
            }
            return null;
        }

        public static TAttribute GetCustomAttribute<TAttribute>(this ICustomAttributeReflectorProvider customAttributeReflectorProvider)
           where TAttribute : Attribute
        {
            return (TAttribute)GetCustomAttribute(customAttributeReflectorProvider, typeof(TAttribute));
        }
    }
}
