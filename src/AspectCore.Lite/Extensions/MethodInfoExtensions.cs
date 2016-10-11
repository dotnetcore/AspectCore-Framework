using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AspectCore.Lite.Extensions
{
    internal static class MethodInfoExtensions
    {
        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            Type returnType = methodInfo.ReturnType;
            return returnType == typeof(Task) ||
                (returnType.GetTypeInfo().IsGenericType && returnType.GetTypeInfo().GetGenericTypeDefinition() == typeof(Task<>));
        }

        internal static bool IsAsync(this MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            return methodInfo.IsDefined(typeof(AsyncStateMachineAttribute));
        }
    }
}