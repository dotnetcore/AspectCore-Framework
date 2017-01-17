using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Abstractions.Extensions
{
    internal static class MethodInfoConstant
    {
        private static readonly Type ILGenType = typeof(Expression).GetTypeInfo().Assembly.GetType("System.Linq.Expressions.Compiler.ILGen");

        internal static readonly MethodInfo EmitConvertToType = ILGenType.GetTypeInfo().GetMethod("EmitConvertToType", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo GetTypeFromHandle = MethodInfoHelpers.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GetMethodFromHandle = MethodInfoHelpers.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
    }
}
