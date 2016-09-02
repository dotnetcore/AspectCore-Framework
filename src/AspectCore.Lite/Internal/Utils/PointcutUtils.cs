using AspectCore.Lite.Core;
using AspectCore.Lite.Core.Async;
using AspectCore.Lite.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal.Utils
{
    internal static class PointcutUtils
    {
        private readonly static TypeInfo InterceptorTypeInfo = typeof(IInterceptor).GetTypeInfo();
        private readonly static TypeInfo AsyncInterceptorTypeInfo = typeof(IAsyncInterceptor).GetTypeInfo();
        private readonly static ConcurrentDictionary<MethodInfo, bool> pointcutCache = new ConcurrentDictionary<MethodInfo, bool>();
        private readonly static IPointcut virtualMethodPointcut = new VirtualMethodPointcut();
        private readonly static IPointcut interfacePointcut = new InterfacePointcut();

        internal static bool IsMemberMatch(MemberInfo member)
        {
            if (member == null) return false;
            MethodInfo method = member as MethodInfo;
            bool isAsync = method == null ? true : method.IsAsync();
            return member.CustomAttributes.Any(data => IsAssignableFrom(data.AttributeType, isAsync));
        }

        private static bool IsAssignableFrom(Type attributeType, bool isAsync)
        {
            if (InterceptorTypeInfo.IsAssignableFrom(attributeType)) return true;
            if (AsyncInterceptorTypeInfo.IsAssignableFrom(attributeType) && isAsync) return true;
            return false;
        }

        internal static bool IsMatchCache(MethodInfo method, Func<MethodInfo, bool> vauleFactory)
        {
            return pointcutCache.GetOrAdd(method, vauleFactory);
        }

        internal static IPointcut GetPointcut(TypeInfo typeInfo)
        {
            if (typeInfo.IsClass)
            {
                return virtualMethodPointcut;
            }
            else if (typeInfo.IsInterface)
            {
                return interfacePointcut;
            }

            throw new ArgumentException("type must be interface or class", nameof(typeInfo));
        }
    }
}
