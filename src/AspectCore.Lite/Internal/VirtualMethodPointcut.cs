using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using AspectCore.Lite.Internal.Utils;

namespace AspectCore.Lite.Internal
{
    public class VirtualMethodPointcut : IPointcut
    {
        public bool IsMatch(MethodInfo method)
        {
            return PointcutUtils.IsMatchCache(method , IsMatchCache);
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

            if (!method.IsVirtual) return false;

            if (PointcutUtils.IsMemberMatch(method , declaringTypeInfo)) return true;

            return false;
        }
    }
}
