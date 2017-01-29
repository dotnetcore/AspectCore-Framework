using System;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    public static class PropertyInfoExtensions
    {
        public static object FastGetValue(this PropertyInfo property, object instance)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            return new PropertyAccessor(property).CreatePropertyGetter()(instance);
        }

        public static TReturn FastGetValue<TReturn>(this PropertyInfo property, object instance)
        {
            return (TReturn)FastGetValue(property, instance);
        }

        public static void FastSetValue(this PropertyInfo property, object instance, object value)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            new PropertyAccessor(property).CreatePropertySetter()(instance, value);
        }

        public static bool IsVirtual(this PropertyInfo property)
        {
            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }
            return (property.CanRead && property.GetMethod.IsVirtual) || (property.CanWrite && property.SetMethod.IsVirtual);
        }
    }
}
