using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Extensions.Reflection;
using AspectCore.Utils;

namespace AspectCore.DynamicProxy
{
    public static class AspectContextRuntimeExtensions
    {
        private static readonly ConcurrentDictionary<MethodInfo, bool> isAsyncCache = new ConcurrentDictionary<MethodInfo, bool>();

        internal static readonly ConcurrentDictionary<MethodInfo, MethodReflector> reflectorTable = new ConcurrentDictionary<MethodInfo, MethodReflector>();

        public static async Task AwaitIfAsync(this AspectContext aspectContext, object returnValue)
        {
            if (returnValue == null)
            {
                return;
            }
            if (returnValue is Task task)
            {
                try
                {
                    await task;
                }
                catch (Exception ex)
                {
                    throw aspectContext.InvocationException(ex);
                }
            }
        }

        public static AspectInvocationException InvocationException(this AspectContext aspectContext, Exception exception)
        {
            if (exception is AspectInvocationException aspectInvocationException)
            {
                return aspectInvocationException;
            }
            return new AspectInvocationException(aspectContext, exception);
        }

        public static bool IsAsync(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            var isAsyncFromMetaData = isAsyncCache.GetOrAdd(aspectContext.ServiceMethod, IsAsyncFromMetaData);
            if (isAsyncFromMetaData)
            {
                return true;
            }
            if (aspectContext.ReturnValue != null)
            {
                return IsAsyncType(aspectContext.ReturnValue.GetType().GetTypeInfo());
            }
            return false;
        }

        public static object UnwrapAsyncReturnValue(this AspectContext aspectContext)
        {
            if (aspectContext == null)
            {
                throw new ArgumentNullException(nameof(aspectContext));
            }
            if (!aspectContext.IsAsync())
            {
                throw new AspectInvocationException(aspectContext, new InvalidOperationException("This operation only support asynchronous method."));
            }
            var returnValue = aspectContext.ReturnValue;
            if (returnValue == null)
            {
                return null;
            }
            var returnTypeInfo = returnValue.GetType().GetTypeInfo();
            return Unwrap(returnValue, returnTypeInfo);
        }

        private static object Unwrap(object value, TypeInfo valueTypeInfo)
        {


            object result = null;

            if (valueTypeInfo.IsTaskWithResult())
            {
                var resultReflector = valueTypeInfo.GetProperty("Result").GetReflector();
                result = resultReflector.GetValue(value);
            }
            else if (valueTypeInfo.IsValueTask())
            {
                result = value;
            }

            else if (value is Task<Task> task)
            {
                return task.Result;
            }
            else if (value is Task)
            {
                return null;
            }
            else
            {
                result = value;
            }

            if (result == null)
            {
                return null;
            }

            var resultTypeInfo = result.GetType().GetTypeInfo();
            if (IsAsyncType(resultTypeInfo))
            {
                return Unwrap(result, resultTypeInfo);
            }
            return result;
        }

        private static bool IsAsyncFromMetaData(MethodInfo method)
        {
            if (IsAsyncType(method.ReturnType.GetTypeInfo()))
            {
                return true;
            }
            if (method.IsDefined(typeof(AsyncAspectAttribute), true))
            {
                if (method.ReturnType == typeof(object))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsAsyncType(TypeInfo typeInfo)
        {
            //return typeInfo.IsTask() || typeInfo.IsTaskWithResult() || typeInfo.IsValueTask();
            if (typeInfo.IsTask())
            {
                return true;
            }
            if (typeInfo.IsTaskWithResult())
            {
                return true;
            }
            if (typeInfo.IsValueTask())
            {
                return true;
            }
            return false;
        }
    }
}