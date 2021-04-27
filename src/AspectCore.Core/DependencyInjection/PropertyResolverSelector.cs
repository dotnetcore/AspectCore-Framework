using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DependencyInjection
{
    /// <summary>
    /// 查询对象属性是否标记了FromServiceContextAttribute特性,并获取关联的属性解析对象
    /// </summary>
    internal sealed class PropertyResolverSelector
    {
        private readonly ConcurrentDictionary<Type, PropertyResolver[]> propertyInjectorCache = new ConcurrentDictionary<Type, PropertyResolver[]>();

        internal static readonly PropertyResolverSelector Default = new PropertyResolverSelector();

        /// <summary>
        /// 查询对象属性是否标记了FromServiceContextAttribute特性,并获取关联的属性解析对象
        /// </summary>
        /// <param name="implementationType">待查询的类型</param>
        /// <returns>属性解析器数组</returns>
        internal PropertyResolver[] SelectPropertyResolver(Type implementationType)
        {
            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }
            return propertyInjectorCache.GetOrAdd(implementationType, type => SelectPropertyResolverInternal(type).ToArray());
        }

        /// <summary>
        /// 查询对象属性是否标记了FromServiceContextAttribute特性,并获取关联的属性解析对象
        /// </summary>
        /// <param name="type">待查询的类型</param>
        /// <returns>属性解析器集合</returns>
        private IEnumerable<PropertyResolver> SelectPropertyResolverInternal(Type type)
        {
            foreach (var property in type.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (property.CanWrite)
                {
                    var reflector = property.GetReflector();
                    if (reflector.IsDefined(typeof(FromServiceContextAttribute)))
                    {
                        yield return new PropertyResolver(provider => provider.GetService(property.PropertyType), reflector);
                    }
                }
            }
        }
    }
}