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
            return ReflectorCacheUtils<Pair<MethodInfo, CallOptions>, MethodReflector>.GetOrAdd(new Pair<MethodInfo, CallOptions>(reflectionInfo, callOption), CreateInternal);

            MethodReflector CreateInternal(Pair<MethodInfo, CallOptions> item)
            {
                var methodInfo = item.Item1;
                if (methodInfo.ContainsGenericParameters)
                {
                    return new OpenGenericMethodReflector(methodInfo);
                }
                if (methodInfo.IsStatic)
                {
                    return new StaticMethodReflector(methodInfo);
                }
                if (methodInfo.DeclaringType.GetTypeInfo().IsValueType || callOption == CallOptions.Call)
                {
                    return new CallMethodReflector(methodInfo);
                }
                return new MethodReflector(methodInfo);
            }
        }
    }
}
