using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Common
{
    internal static class MethodInfoConstant
    {
        private static readonly Type ILGENTYPE = typeof(Expression).GetTypeInfo().Assembly.GetType("System.Linq.Expressions.Compiler.ILGen");

        internal static readonly MethodInfo EMITCONVERTTOTYPE = ILGENTYPE.GetTypeInfo().GetMethod("EmitConvertToType", BindingFlags.NonPublic | BindingFlags.Static);

        internal static readonly MethodInfo GETTYPEFROMHANDLE = MethodInfoHelpers.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GETMETHODFROMHANDLE = MethodInfoHelpers.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ARGUMENTNULLEXCEPTIONCCTOR = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
    }
}
