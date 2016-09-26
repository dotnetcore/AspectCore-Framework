using AspectCore.Lite.Core;
using AspectCore.Lite.Internal.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Abstractions.Extensions
{
    internal static class TypeInfoExtensions
    {
        internal static IEnumerable<MethodInfo> GetPointcutMethod(this TypeInfo typeInfo, IPointcut pointCut)
        {
            return typeInfo.DeclaredMethods.Where(method => pointCut.IsMatch(method));
        }

        internal static bool CanProxy(this TypeInfo typeInfo)
        {
            IPointcut pointcut = PointcutUtils.GetPointcut(typeInfo);
            return typeInfo.DeclaredMethods.Any(method => pointcut.IsMatch(method));
        }
    }
}
