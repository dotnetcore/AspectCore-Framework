using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.DependencyInjection;

namespace AspectCore.Utils
{
    internal static class PropertyInjectionUtils
    {
        private readonly static ConcurrentDictionary<Type, bool> dictionary = new ConcurrentDictionary<Type, bool>();

        public static bool TypeRequired(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return dictionary.GetOrAdd(type, _ => type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanWrite).Any(x => x.GetReflector().IsDefined<FromServiceContextAttribute>()));
        }

        public static bool Required(object instance)
        {
            if (instance == null)
            {
                return false;
            }
            return TypeRequired(instance.GetType());
        }
    }
}