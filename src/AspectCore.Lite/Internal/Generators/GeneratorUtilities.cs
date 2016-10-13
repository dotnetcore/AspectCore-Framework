using AspectCore.Lite.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Generators
{
    internal static class GeneratorUtilities
    {
        private static readonly HashSet<string> NonOverrideMethodList = new HashSet<string> { "Equals", "GetHashCode", "ToString" };
        internal static MethodInfo GetMethodInfo<T>(Expression<T> expr)
        {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }

        internal static bool IsPropertyMethod(MethodInfo method, Type serviceType) =>
            serviceType.GetTypeInfo().DeclaredProperties.Any(property => (property.CanRead && property.GetMethod == method) || (property.CanWrite && property.SetMethod == method));

        internal static bool IsOverridedMethod(MethodInfo method, Type serviceType)
        {
            var pointcut = PointcutUtilities.GetPointcut(serviceType.GetTypeInfo());
            return pointcut.IsMatch(method) && !NonOverrideMethodList.Contains(method.Name);
        }
    }
}
