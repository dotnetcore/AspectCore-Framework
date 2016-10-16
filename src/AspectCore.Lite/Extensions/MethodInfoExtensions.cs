using AspectCore.Lite.Abstractions;
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
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }

            var returnType = methodInfo.ReturnType;

            return typeof(Task).GetTypeInfo().IsAssignableFrom(returnType);
        }
    }
}