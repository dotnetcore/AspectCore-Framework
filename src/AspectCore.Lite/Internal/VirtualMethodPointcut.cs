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
            return PointcutUtils.IsMatchCache(method, key =>
            {
                if (key == null) return false;

                if (!key.IsVirtual) return false;

                TypeInfo declaringTypeInfo = key.DeclaringType.GetTypeInfo();

                if (!declaringTypeInfo.IsClass)
                    throw new ArgumentException("DeclaringType should be class", nameof(method));

                if (PointcutUtils.IsMemberMatch(declaringTypeInfo)) return true;

                if (PointcutUtils.IsMemberMatch(key)) return true;

                return false;
            });
        }
    }
}
