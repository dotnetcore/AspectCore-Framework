using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector
    {
        internal static MethodReflector Create(MethodInfo reflectionInfo, CallOptions callOption)
        {
            if (reflectionInfo == null)
            {
                throw new ArgumentNullException(nameof(reflectionInfo));
            }
            return ReflectorCacheUtils<Tuple<MethodInfo, CallOptions>, MethodReflector>.GetOrAdd(Tuple.Create(reflectionInfo, callOption), CreateInternal);

            MethodReflector CreateInternal(Tuple<MethodInfo, CallOptions> item)
            {
                var methodInfo = item.Item1;
                if (methodInfo.ContainsGenericParameters)
                {
                    return new OpenGenericMethodReflector(item.Item1);
                }
                if (methodInfo.IsStatic)
                {
                    return new StaticMethodReflector(methodInfo);
                }
                if (callOption == CallOptions.Call)
                {
                    return new CallMethodReflector(methodInfo);
                }
                return new MethodReflector(methodInfo);
            }
        }
    }
}
