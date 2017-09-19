using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    internal static class AspectContextRuntimeExtensions
    {
        internal static readonly ConcurrentDictionary<MethodInfo, MethodReflector> reflectorTable = new ConcurrentDictionary<MethodInfo, MethodReflector>();
    }
}