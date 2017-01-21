using System.Reflection;
using System.Threading.Tasks;

namespace AspectCore.Abstractions.Extensions
{
    internal static class MethodInfoExtensions
    {
        internal static bool IsReturnTask(this MethodInfo methodInfo)
        {
            return typeof(Task).GetTypeInfo().IsAssignableFrom(methodInfo.ReturnType.GetTypeInfo());
        }
    }
}
