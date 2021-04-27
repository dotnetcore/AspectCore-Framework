using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AspectCore.Extensions.Reflection.Internals;

namespace AspectCore.Extensions.Reflection
{
    public partial class TypeReflector
    {
        /// <summary>
        /// 通过TypeInfo对象获取对应的TypeReflector对象
        /// </summary>
        /// <param name="typeInfo">类型对象</param>
        /// <returns>类型反射操作</returns>
        internal static TypeReflector Create(TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return ReflectorCacheUtils<TypeInfo, TypeReflector>.GetOrAdd(typeInfo, info => new TypeReflector(info));
        }
    }
}