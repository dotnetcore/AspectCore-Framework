using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public static class MethodExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, PropertyInfo> dictionary = new ConcurrentDictionary<MethodInfo, PropertyInfo>();

        /// <summary>
        /// 如果方法是属性的get或set访问器,则返回true,否则false
        /// </summary>
        /// <param name="method">待判断的方法</param>
        /// <returns>是属性的get或set访问器，则返回true,否则false</returns>
        public static bool IsPropertyBinding(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            return method.GetBindingProperty() != null;
        }

        /// <summary>
        /// 如果方法是属性的get或set访问器,则返回对应的属性,否则返回null
        /// </summary>
        /// <param name="method">待判断的方法</param>
        /// <returns>对应属性</returns>
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