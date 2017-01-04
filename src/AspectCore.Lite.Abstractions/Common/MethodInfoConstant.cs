using System;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Common
{
    internal static class MethodInfoConstant
    {

        internal static readonly MethodInfo GetTypeFromHandle = MethodInfoHelpers.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GetMothodFromHandle = MethodInfoHelpers.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
    }
}
