using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace AspectCore.Lite.Internal.Generators
{
    internal static class GeneratorHelper
    {
        private static readonly HashSet<string> NonOverrideMethodList = new HashSet<string> { "Equals", "GetHashCode", "ToString" };
        internal static MethodInfo GetMethodInfo<T>(Expression<T> expr)
        {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }

        internal static bool IsPropertyMethod(MethodInfo method, Type serviceType) =>
            serviceType.GetTypeInfo().DeclaredProperties.Any(property => (property.CanRead && property.GetMethod == method) || (property.CanWrite && property.SetMethod == method));

        internal static bool IsOverridedMethod(MethodInfo method, IPointcut pointcut)
        {
            return pointcut.IsMatch(method) && !NonOverrideMethodList.Contains(method.Name);
        }

        internal static string GetMethodName(Type serviceType, string method)
        {
            if (!serviceType.GetTypeInfo().IsInterface)
            {
                return method;
            }
            return $"{serviceType.FullName}.{method}".Replace('+', '.');
        }
    }
}
