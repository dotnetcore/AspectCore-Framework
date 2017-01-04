using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Lite.Abstractions.Resolution.Common
{
    internal static class MethodInfoExtensions
    {
        internal static bool IsPropertyMethod(this MethodInfo method)
        {
            return method.DeclaringType.GetTypeInfo().DeclaredProperties.Any(
                property => (property.CanRead && property.GetMethod == method) || (property.CanWrite && property.SetMethod == method));
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
            return typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo());
        }

       
    }
}
