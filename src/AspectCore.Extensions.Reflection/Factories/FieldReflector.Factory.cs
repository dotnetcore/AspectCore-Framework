using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class FieldReflector
    {
        /// <summary>
        /// 通过FieldInfo对象获取对应的FieldReflector对象
        /// </summary>
        /// <param name="reflectionInfo">字段</param>
        /// <returns>字段反射操作</returns>
        internal static FieldReflector Create(FieldInfo reflectionInfo)
        {
            if (reflectionInfo == null)
            {
                throw new ArgumentNullException(nameof(reflectionInfo));
            }

            return ReflectorCacheUtils<FieldInfo, FieldReflector>.GetOrAdd(reflectionInfo, CreateInternal);

            FieldReflector CreateInternal(FieldInfo field)
            {
                if (field.DeclaringType.GetTypeInfo().ContainsGenericParameters)
                {
                    return new OpenGenericFieldReflector(field);
                }

                if (field.DeclaringType.IsEnum)
                {
                    return new EnumFieldReflector(field);
                }

                if (field.IsStatic)
                {
                    return new StaticFieldReflector(field);
                }

                return new FieldReflector(field);
            }
        }
    }
}
