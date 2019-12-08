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
                 foreach (var property in m.DeclaringType.GetTypeInfo().GetProperties())
                 {
                     if (property.CanRead && property.GetMethod == m)
                     {
                         return property;
                     }

                     if (property.CanWrite && property.SetMethod == m)
                     {
                         return property;
                     }
                 }
                 return null;
             });
        }
    }
}