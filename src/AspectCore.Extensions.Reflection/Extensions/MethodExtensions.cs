using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public static class MethodExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, PropertyInfo> dictionary = new ConcurrentDictionary<MethodInfo, PropertyInfo>();

        public static bool IsPropertyBinding(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetBindingProperty() != null;
        }

        public static PropertyInfo GetBindingProperty(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }

            return dictionary.GetOrAdd(method, m =>
            {
                // the method may be a reflected method, so get the base definition and then check equality.
                var baseDef = method.GetBaseDefinition();
                foreach (var property in m.DeclaringType.GetTypeInfo().GetProperties())
                {
                    if (property.CanRead && property.GetMethod == baseDef)
                    {
                        return property;
                    }

                    if (property.CanWrite && property.SetMethod == baseDef)
                    {
                        return property;
                    }
                }
                return null;
            });
        }
    }
}