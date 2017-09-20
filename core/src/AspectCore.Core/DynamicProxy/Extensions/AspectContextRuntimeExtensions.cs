using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;

namespace AspectCore.DynamicProxy
{
    internal static class AspectContextRuntimeExtensions
    {
        internal static readonly ConcurrentDictionary<MethodInfo, MethodReflector> reflectorTable = new ConcurrentDictionary<MethodInfo, MethodReflector>();

        public static void AwaitIfAsync(this AspectContext aspectContext, object returnValue)
        {
            if (returnValue == null)
            {
                return;
            }
            if (returnValue is Task task)
            {
                if (task.IsFaulted)
                {
                    var innerException = task.Exception?.InnerException;
                    throw aspectContext.InvocationException(innerException);
                }
                if (!task.IsCompleted)
                {
                    task.GetAwaiter().GetResult();
                }
            }
        }

        internal static AspectInvocationException InvocationException(this AspectContext aspectContext, Exception exception)
        {
            if (exception is AspectInvocationException aspectInvocationException)
            {
                throw new AspectInvocationException(aspectContext, aspectInvocationException.InnerException);
            }
            return new AspectInvocationException(aspectContext, exception);
        }
    }
}