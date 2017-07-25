using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class ConstructorReflector
    {
        internal static ConstructorReflector Create(ConstructorInfo constructorInfo)
        {
            if (constructorInfo == null)
            {
                throw new ArgumentNullException(nameof(constructorInfo));
            }
            return ReflectorCacheUtils<ConstructorInfo, ConstructorReflector>.GetOrAdd(constructorInfo, info => new ConstructorReflector(info));
        }
    }
}
