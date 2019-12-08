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

        public static ValueTask AwaitIfAsync(this AspectContext aspectContext)
        {
            return AwaitIfAsync(aspectContext, aspectContext.ReturnValue);
        }

        public static async ValueTask AwaitIfAsync(this AspectContext aspectContext, object returnValue)
        {
            switch (returnValue)
            {
                case null:
                    break;
                case Task task:
                    await task;
                    break;
                case ValueTask valueTask:
                    await valueTask;
                    break;
                default:
                    if (returnValue.GetType().GetTypeInfo().IsValueTaskWithResult())
                    {
                        await (dynamic) returnValue;
                    }
                    break;
            }
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

        public static async Task<T> UnwrapAsyncReturnValue<T>(this AspectContext aspectContext)
        {
            return (T) await UnwrapAsyncReturnValue(aspectContext);
        }

        public static Task<object> UnwrapAsyncReturnValue(this AspectContext aspectContext)
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

        private static async Task<object> Unwrap(object value, TypeInfo valueTypeInfo)
        {
            object result = null;

            if (valueTypeInfo.IsTaskWithResult())
            {
                // Is there better solution to unwrap ?
                result = (object) (await (dynamic) value);
            }
            else if (valueTypeInfo.IsValueTaskWithResult())
            {
                // Is there better solution to unwrap ?
                result = (object) (await (dynamic) value);
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

            if (typeInfo.IsValueTaskWithResult())
            {
                return true;
            }

            return false;
        }
    }
}