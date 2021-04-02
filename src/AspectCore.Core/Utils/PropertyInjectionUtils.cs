using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;
using AspectCore.DependencyInjection;

namespace AspectCore.Utils
{
    /// <summary>
    /// 属性注入工具类
    /// </summary>
    internal static class PropertyInjectionUtils
    {
        private readonly static ConcurrentDictionary<Type, bool> dictionary = new ConcurrentDictionary<Type, bool>();

        /// <summary>
        /// 判断类型中是否存在标注了FromServiceContextAttribute的属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>如果全部属性都没有标注FromServiceContextAttribute特性,则返回false。否则返回true</returns>
        public static bool TypeRequired(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            return dictionary.GetOrAdd(type, _ => type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(x => x.CanWrite).Any(x => x.GetReflector().IsDefined<FromServiceContextAttribute>()));
        }

        /// <summary>
        /// 判断对象中是否存在标注了FromServiceContextAttribute的属性
        /// </summary>
        /// <param name="type">类型</param>
        /// <returns>如果全部属性都没有标注FromServiceContextAttribute特性,则返回false。否则返回true</returns>
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