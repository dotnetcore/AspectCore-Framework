using System;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Common
{
    internal static class MethodConstant
    {

        internal static readonly MethodInfo GetTypeFromHandle = GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GetMothodFromHandle = GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });

        private static MethodInfo GetMethod<T>(Expression<T> expr)
        {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }
    }
}
