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

        internal static string ExplicitName(this MethodInfo method)
        {
            var declaringType = method.DeclaringType.GetTypeInfo();
            if (declaringType.IsInterface)
            {
                return $"{declaringType.FullName}.{method.Name}".Replace('+', '.');
            }
            return method.Name;
        }

        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            return typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo());
        }
    }
}
