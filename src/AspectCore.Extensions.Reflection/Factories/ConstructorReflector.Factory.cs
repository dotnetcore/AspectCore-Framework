using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class ConstructorReflector
    {
        /// <summary>
        /// 通过ConstructorInfo对象获取对应的ConstructorReflector对象
        /// </summary>
        /// <param name="constructorInfo">构造器</param>
        /// <returns>构造方法反射调用</returns>
        internal static ConstructorReflector Create(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException(nameof(constructorInfo));
            }
            return ReflectorCacheUtils<ConstructorInfo, ConstructorReflector>.GetOrAdd(constructorInfo, info =>
            {
                if (info.DeclaringType.GetTypeInfo().ContainsGenericParameters)
                {
                    return new OpenGenericConstructorReflector(info);
                }
                return new ConstructorReflector(info);
            });
        }
    }
}