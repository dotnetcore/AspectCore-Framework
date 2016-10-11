using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Extensions
{
    internal static class TypeInfoExtensions
    {
        internal static IEnumerable<MethodInfo> GetPointcutMethod(this TypeInfo typeInfo, IPointcut pointCut)
        {
            return typeInfo.DeclaredMethods.Where(method => pointCut.IsMatch(method));
        }

        internal static bool CanProxy(this TypeInfo typeInfo)
        {
            IPointcut pointcut = PointcutUtilities.GetPointcut(typeInfo);
            return typeInfo.DeclaredMethods.Any(method => pointcut.IsMatch(method));
        }

        internal static MethodInfo GetRequiredMethod(this Type type, string name, Type[] parameterTypes)
        {
            var method = type.GetTypeInfo().GetMethod(name, parameterTypes);

            if(method==null)
            {
                throw new MissingMethodException($"Not found method named {name} in {type}");
            }

            return method;
        }
    }
}
