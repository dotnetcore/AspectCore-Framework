using AspectCore.Lite.Abstractions.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Extensions
{
    internal static class TypeInfoExtensions
    {
        internal static IEnumerable<MethodInfo> GetJoinPointMethod(this TypeInfo typeInfo, IPointcut pointCut)
        {
            return typeInfo.DeclaredMethods.Where(method => pointCut.IsMatch(method));
        }
    }
}
