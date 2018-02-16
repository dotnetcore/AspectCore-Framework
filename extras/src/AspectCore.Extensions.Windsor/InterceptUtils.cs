using System.Linq;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;
using System.Collections.Concurrent;
using System.Reflection;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.Windsor
{
    internal static class InterceptUtils
    {
        private static readonly MethodInfo invoke = typeof(IAspectActivator).GetMethod(nameof(IAspectActivator.Invoke));
        private static readonly MethodInfo invokeTask = typeof(IAspectActivator).GetMethod(nameof(IAspectActivator.InvokeTask));
        private static readonly MethodInfo invokeValueTask = typeof(IAspectActivator).GetMethod(nameof(IAspectActivator.InvokeValueTask));

        private static readonly ConcurrentDictionary<MethodInfo, MethodReflector> Invokes = new ConcurrentDictionary<MethodInfo, MethodReflector>();

        internal static MethodReflector GetInvokeReflector(MethodInfo method)
        {
            return Invokes.GetOrAdd(method, GetInvokeReflectorInternal);
        }

        private static MethodReflector GetInvokeReflectorInternal(MethodInfo method)
        {
            if (method.ReturnType == typeof(void))
            {
                return invoke.MakeGenericMethod(typeof(object)).GetReflector();
            }
            else if (method.ReturnType == typeof(Task))
            {
                return invokeTask.MakeGenericMethod(typeof(object)).GetReflector();
            }
            else if (method.IsReturnTask())
            {
                var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                return invokeTask.MakeGenericMethod(returnType).GetReflector();
            }
            else if (method.IsReturnValueTask())
            {
                var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                return invokeValueTask.MakeGenericMethod(returnType).GetReflector();
            }
            else
            {
                return invoke.MakeGenericMethod(method.ReturnType).GetReflector();
            }
        }
    }
}