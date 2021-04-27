using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class CustomAttributeReflector
    {
        /// <summary>
        /// 通过CustomAttributeData对象获取对应的CustomAttributeReflector对象
        /// </summary>
        /// <param name="customAttributeData">提供对加载到仅反射上下文的程序集、模块、类型、成员和参数的自定义属性数据的访问权限</param>
        /// <returns>自定义特性反射操作</returns>
        internal static CustomAttributeReflector Create(CustomAttributeData customAttributeData)
        {
            return ReflectorCacheUtils<CustomAttributeData, CustomAttributeReflector>.GetOrAdd(customAttributeData, data => new CustomAttributeReflector(data));
        }
    }
}
