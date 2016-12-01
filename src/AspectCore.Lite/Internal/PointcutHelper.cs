using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using static AspectCore.Lite.Internal.Pointcut;

namespace AspectCore.Lite.Internal
{
    internal static class PointcutHelper
    {
        private readonly static TypeInfo InterceptorTypeInfo = typeof(IInterceptor).GetTypeInfo();
        private readonly static ConcurrentDictionary<MethodInfo, bool> pointcutCache = new ConcurrentDictionary<MethodInfo, bool>();
       
        internal static bool IsMemberMatch(MethodInfo method, TypeInfo declaringTypeInfo)
        {
            if (method.CustomAttributes.Any(data => IsAssignableFrom(data.AttributeType)))
            {
                return true;
            }
            return declaringTypeInfo.CustomAttributes.Any(data => IsAssignableFrom(data.AttributeType));
        }

        internal static bool IsGlobalMatch(IInterceptorCollection interceptorCollection)
        {
            return interceptorCollection.Any();
        }

        internal static bool IsNonAspect(MethodInfo method, TypeInfo declaringTypeInfo)
        {
            if (method.IsDefined(typeof(NonAspectAttribute), true))
            {
                return true;
            }
            return declaringTypeInfo.IsDefined(typeof(NonAspectAttribute), true);
        }

        private static bool IsAssignableFrom(Type attributeType) => InterceptorTypeInfo.IsAssignableFrom(attributeType);

        internal static bool IsMatchCache(MethodInfo method, Func<MethodInfo, bool> vauleFactory) => pointcutCache.GetOrAdd(method, vauleFactory);

        internal static IPointcut GetPointcut(TypeInfo typeInfo, IInterceptorCollection interceptorCollection)
        {
            if (typeInfo.IsClass)
            {
                return new VirtualMethodPointcut(interceptorCollection);
            }
            else if (typeInfo.IsInterface)
            {
                return new InterfacePointcut(interceptorCollection);
            }

            throw new ArgumentException("Type must be interface or class", nameof(typeInfo));
        }
    }
}
