using AspectCore.Lite.Abstractions;
using System;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal class VirtualMethodPointcut : IPointcut
    {
        public bool IsMatch(MethodInfo method)
        {
            return PointcutHelpers.IsMatchCache(method , IsMatchCache);
        }

        private bool IsMatchCache(MethodInfo method)
        {
            if (method == null) return false;

            TypeInfo declaringTypeInfo = method.DeclaringType.GetTypeInfo();

            if (!declaringTypeInfo.IsClass)
                throw new ArgumentException("DeclaringType should be class" , nameof(method));

            if (declaringTypeInfo.IsSealed)
                throw new ArgumentException("DeclaringType cannot be sealed" , nameof(method));

            if (method.IsStatic) return false;

            if (method.IsPrivate) return false;

            if (!method.IsVirtual) return false;

            if (PointcutHelpers.IsMemberMatch(method , declaringTypeInfo)) return true;

            return false;
        }
    }
}
