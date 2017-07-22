using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector
    {
        internal static MethodReflector Create(MethodInfo reflectionInfo, bool callVirt)
        {
            if (reflectionInfo == null)
            {
                throw new ArgumentNullException(nameof(reflectionInfo));
            }
            return ReflectorCache<Tuple<MethodInfo, bool>, MethodReflector>.GetOrAdd(Tuple.Create(reflectionInfo, callVirt), info => new MethodReflector(reflectionInfo));
        }
    }
}
