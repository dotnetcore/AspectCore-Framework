using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;

namespace AspectCore.Lite.Internal
{
    internal class DefaultPointcut : IPointcut
    {

        public bool IsMatch(MethodInfo method)
        {
            if (method == null)
            {
                return false;
            }

            var pointcut = PointcutUtilities.GetPointcut(method.DeclaringType.GetTypeInfo());
            return pointcut.IsMatch(method);
        }

        internal class InterfacePointcut : IPointcut
        {
            public bool IsMatch(MethodInfo method)
            {
                return PointcutUtilities.IsMatchCache(method, IsMatchCache);
            }

            private bool IsMatchCache(MethodInfo method)
            {
                if (method == null)
                {
                    return false;
                }

                TypeInfo declaringTypeInfo = method.DeclaringType.GetTypeInfo();

                if (!declaringTypeInfo.IsInterface)
                {
                    return false;
                }

                return PointcutUtilities.IsMemberMatch(method, declaringTypeInfo);
            }
        }

        internal class VirtualMethodPointcut : IPointcut
        {
            public bool IsMatch(MethodInfo method)
            {
                return PointcutUtilities.IsMatchCache(method, IsMatchCache);
            }

            private bool IsMatchCache(MethodInfo method)
            {
                if (method == null)
                {
                    return false;
                }

                TypeInfo declaringTypeInfo = method.DeclaringType.GetTypeInfo();

                if (!declaringTypeInfo.IsClass || declaringTypeInfo.IsSealed)
                {
                    return false;
                }

                if (method.IsStatic || !method.IsVirtual || method.IsFinal)
                {
                    return false;
                }

                if (!method.IsFamilyOrAssembly && !method.IsPublic && !method.IsFamily)
                {
                    return false;
                }

                return PointcutUtilities.IsMemberMatch(method, declaringTypeInfo);
            }
        }
    }
}
