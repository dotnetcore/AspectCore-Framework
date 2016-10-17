using AspectCore.Lite.Abstractions;
using AspectCore.Lite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AspectCore.Lite.Extensions
{
    public static class TypeInfoExtensions
    {

        public static bool CanProxy(this TypeInfo typeInfo)
        {
            ExceptionUtilities.ThrowArgumentNull(typeInfo , nameof(typeInfo));

            if (typeInfo.IsValueType)
            {
                return false;
            }

            var pointcut = PointcutUtilities.GetPointcut(typeInfo);
            return typeInfo.DeclaredMethods.Any(method => pointcut.IsMatch(method));
        }
    }
}
