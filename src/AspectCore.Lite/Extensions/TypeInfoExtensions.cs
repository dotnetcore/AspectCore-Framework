using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Internal;
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
            IPointcut pointcut = PointcutHelpers.GetPointcut(typeInfo);
            return typeInfo.DeclaredMethods.Any(method => pointcut.IsMatch(method));
        }
    }
}
