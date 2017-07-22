using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Extensions.Reflection
{
    internal static class TypeConstants
    {
        internal readonly static Type ObjectType = typeof(object);

        internal readonly static Type[] ConstructorInvokerParameter = new Type[] { typeof(object[]) };

        internal readonly static Type ConstructorInvokerType = typeof(Func<object[], object>);
    }

    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GetTypeFromHandle = ReflectionExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GetMethodFromHandle = ReflectionExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });

        internal static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
    }
}
