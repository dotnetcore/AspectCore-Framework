using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.DynamicProxy.Common
{
    internal static class MethodHelper
    {
        private static readonly HashSet<string> NonOverrideMethodList = new HashSet<string> { "Equals", "GetHashCode", "ToString" };

        internal static bool IsPropertyMethod(this MethodInfo method)
        {
            return method.DeclaringType.GetTypeInfo().DeclaredProperties.Any(
                property => (property.CanRead && property.GetMethod == method) || (property.CanWrite && property.SetMethod == method));
        }

        internal static bool IsIgnored(this MethodInfo method)
        {
            return NonOverrideMethodList.Contains(method.Name);
        }

        internal static string ConvertMethodNameIfExplicit(Type serviceType, string method)
        {
            if (serviceType.GetTypeInfo().IsInterface)
            {
                return $"{serviceType.FullName}.{method}".Replace('+', '.');
            }
            return method;
        }

        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            return typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType);
        }

        internal static MethodInfo GetMethod<T>(Expression<T> expr)
        {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }

        internal static MethodInfo GetMethod<T>(string name)
        {
            return typeof(T).GetTypeInfo().GetMethod(name);
        }
    }
}
