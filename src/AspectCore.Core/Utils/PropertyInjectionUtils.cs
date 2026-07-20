using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
            return dictionary.GetOrAdd(type, _ =>
            {
                var props = type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.CanWrite);
                if (!RuntimeFeature.IsDynamicCodeSupported)
                {
                    // NativeAOT: use standard reflection
                    return props.Any(x => x.IsDefined(typeof(FromServiceContextAttribute), true));
                }
                return props.Any(x => x.GetReflector().IsDefined<FromServiceContextAttribute>());
            });
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