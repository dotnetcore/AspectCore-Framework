using AspectCore.Lite.Abstractions;
using System;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal class InterfacePointcut : IPointcut
    {
        public bool IsMatch(MethodInfo method)
        {
            return PointcutUtilities.IsMatchCache(method, IsMatchCache);
        }

        private bool IsMatchCache(MethodInfo method)
        {
            if (method == null) return false;
            TypeInfo declaringTypeInfo = method.DeclaringType.GetTypeInfo();
            return declaringTypeInfo.IsInterface && PointcutUtilities.IsMemberMatch(method, declaringTypeInfo);
        }
    }
}
