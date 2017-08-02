using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace AspectCore.Extensions.Reflection
{
    public static class CustomAttributeExtensions
    {
        private static readonly Attribute[] empty = new Attribute[0];

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
                return empty;
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
                return empty;
            }
            var checkedAttrs = new Attribute[customAttributeLength];
            var @checked = 0;
            var attrToken = attributeType.GetTypeInfo().MetadataToken;
            for (var i = 0; i < customAttributeLength; i++)
            {
                var reflector = customAttributeReflectors[i];
                if (ContainsToken(reflector._tokens, attrToken))
                    checkedAttrs[@checked++] = reflector.Invoke();
            }
            if (customAttributeLength == @checked)
            {
                return checkedAttrs;
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
            var attrToken = typeof(TAttribute).GetTypeInfo().MetadataToken;
            for (var i = 0; i < customAttributeLength; i++)
            {
                var reflector = customAttributeReflectors[i];
                if (ContainsToken(reflector._tokens, attrToken))
                    checkedAttrs[@checked++] = (TAttribute)reflector.Invoke();
            }
            if (customAttributeLength == @checked)
            {
                return checkedAttrs;
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
            if (attributeType == null)
            {
                throw new ArgumentNullException(nameof(attributeType));
            }
            var customAttributeReflectors = customAttributeReflectorProvider.CustomAttributeReflectors;
            var customAttributeLength = customAttributeReflectors.Length;
            if (customAttributeLength == 0)
            {
                return null;
            }
            var attrToken = attributeType.GetTypeInfo().MetadataToken;
            for (var i = 0; i < customAttributeLength; i++)
            {
                var reflector = customAttributeReflectors[i];
                if (ContainsToken(reflector._tokens, attrToken))
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

        public static bool IsDefined(this ICustomAttributeReflectorProvider customAttributeReflectorProvider, Type attributeType)
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
                return false;
            }
            var attrToken = attributeType.GetTypeInfo().MetadataToken;
            for (var i = 0; i < customAttributeLength; i++)
            {
                if (ContainsToken(customAttributeReflectors[i]._tokens, attrToken))
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsDefined<TAttribute>(this ICustomAttributeReflectorProvider customAttributeReflectorProvider) where TAttribute : Attribute
        {
            return IsDefined(customAttributeReflectorProvider, typeof(TAttribute));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool ContainsToken(int[] tokens, int token)
        {
            var length = tokens.Length;
            if (length == 1)
            {
                return tokens[0] == token;
            }
            var first = 0;
            while (first <= length)
            {
                var middle = (first + length) / 2;
                var entry = tokens[middle];
                if (entry == token)
                {
                    return true;
                }
                else if (entry < token)
                {
                    first = middle + 1;
                }
                else
                {
                    length = middle - 1;
                }
            }
            return false;
        }
    }
}