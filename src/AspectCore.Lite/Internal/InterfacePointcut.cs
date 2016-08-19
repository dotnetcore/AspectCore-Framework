using AspectCore.Lite.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;
using AspectCore.Lite.Internal.Utils;

namespace AspectCore.Lite.Internal
{
    internal class InterfacePointcut : IPointcut
    {
        public bool IsMatch(MethodInfo method)
        {
            return PointcutUtils.IsMatchCache(method, key =>
            {
                if (key == null) return false;

                TypeInfo declaringTypeInfo = key.DeclaringType.GetTypeInfo();

                if (!declaringTypeInfo.IsInterface)
                    throw new ArgumentException("DeclaringType should be Interface", nameof(method));

                if (PointcutUtils.IsMemberMatch(declaringTypeInfo)) return true;

                if (PointcutUtils.IsMemberMatch(key)) return true;

                return false;
            });
        }
    }
}
