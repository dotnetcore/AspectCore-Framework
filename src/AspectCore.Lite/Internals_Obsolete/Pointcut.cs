using AspectCore.Lite.Abstractions;
using System.Reflection;

namespace AspectCore.Lite.Internals
{
    internal sealed class Pointcut : IPointcut
    {
        private readonly IInterceptorCollection interceptorCollection;

        public Pointcut(IInterceptorCollection interceptorCollection)
        {
            this.interceptorCollection = interceptorCollection;
        }

        public bool IsMatch(MethodInfo method)
        {
            if (method == null)
            {
                return false;
            }

            var pointcut = PointcutHelper.GetPointcut(method.DeclaringType.GetTypeInfo(), interceptorCollection);
            return pointcut.IsMatch(method);
        }

        internal class InterfacePointcut : IPointcut
        {
            private readonly IInterceptorCollection interceptorCollection;

            public InterfacePointcut(IInterceptorCollection interceptorCollection)
            {
                this.interceptorCollection = interceptorCollection;
            }

            public bool IsMatch(MethodInfo method)
            {
                return PointcutHelper.IsMatchCache(method, IsMatchCache);
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

                if (PointcutHelper.IsNonAspect(method, declaringTypeInfo))
                {
                    return false;
                }

                if (PointcutHelper.IsMemberMatch(method, declaringTypeInfo))
                {
                    return true;
                }

                return PointcutHelper.IsGlobalMatch(interceptorCollection);
            }
        }

        internal class VirtualMethodPointcut : IPointcut
        {
            private readonly IInterceptorCollection interceptorCollection;

            public VirtualMethodPointcut(IInterceptorCollection interceptorCollection)
            {
                this.interceptorCollection = interceptorCollection;
            }

            public bool IsMatch(MethodInfo method)
            {
                return PointcutHelper.IsMatchCache(method, IsMatchCache);
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

                if (PointcutHelper.IsNonAspect(method, declaringTypeInfo))
                {
                    return false;
                }

                if (PointcutHelper.IsMemberMatch(method, declaringTypeInfo))
                {
                    return true;
                }

                return PointcutHelper.IsGlobalMatch(interceptorCollection);
            }
        }
    }
}
