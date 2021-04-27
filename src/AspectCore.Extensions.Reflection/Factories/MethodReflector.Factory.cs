using System;
using System.Reflection;

namespace AspectCore.Extensions.Reflection
{
    public partial class MethodReflector
    {
        /// <summary>
        /// 通过MethodInfo对象和调用方式获取对应的MethodReflector对象
        /// </summary>
        /// <param name="reflectionInfo">方法</param>
        /// <param name="callOption">调用方式</param>
        /// <returns>方法反射调用</returns>
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
