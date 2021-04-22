using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class ParameterReflector
    {
        /// <summary>
        /// 通过ParameterInfo对象获取对应的ParameterReflector对象
        /// </summary>
        /// <param name="parameterInfo">参数</param>
        /// <returns>参数反射操作</returns>
        internal static ParameterReflector Create(ParameterInfo parameterInfo)
        {
            if (parameterInfo == null)
            {
                throw new ArgumentNullException(nameof(parameterInfo));
            }
            return ReflectorCacheUtils<ParameterInfo, ParameterReflector>.GetOrAdd(parameterInfo, info => new ParameterReflector(info));
        }
    }
}