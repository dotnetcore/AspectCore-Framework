using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class ParameterReflector
    {
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