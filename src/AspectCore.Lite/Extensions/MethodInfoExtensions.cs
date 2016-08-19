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
        internal static bool IsAsync(this MethodInfo methodInfo)
        {
            if (methodInfo == null) throw new ArgumentNullException(nameof(methodInfo));
            Type returnType = methodInfo.ReturnType;
            if (!(returnType == typeof(void) || typeof(Task).GetTypeInfo().IsAssignableFrom(returnType))) return false;
            return methodInfo.IsDefined(typeof(AsyncStateMachineAttribute)));
        }
    }
}