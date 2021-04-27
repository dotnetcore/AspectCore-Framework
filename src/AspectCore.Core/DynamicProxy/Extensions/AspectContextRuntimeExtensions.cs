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

        /// <summary>
        /// 等待异步拦截处理
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>结果</returns>
        public static ValueTask AwaitIfAsync(this AspectContext aspectContext)
        {
            return AwaitIfAsync(aspectContext, aspectContext.ReturnValue);
        }

        /// <summary>
        /// 如果returnValue为异步任务，则等待任务完成
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <param name="returnValue">待判断的值</param>
        /// <returns>ValueTask</returns>
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
                        await (dynamic)returnValue;
                    }
                    break;
            }
        }

        /// <summary>
        /// 检查是否为异步拦截处理
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>结果</returns>
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
                //返回值为异步类型
                return IsAsyncType(aspectContext.ReturnValue.GetType().GetTypeInfo());
            }

            return false;
        }

        /// <summary>
        /// 等待并获取异步拦截结果
        /// </summary>
        /// <typeparam name="T">结果类型</typeparam>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>异步拦截结果</returns>
        public static async Task<T> UnwrapAsyncReturnValue<T>(this AspectContext aspectContext)
        {
            return (T)await UnwrapAsyncReturnValue(aspectContext);
        }

        /// <summary>
        /// 等待并获取异步拦截结果
        /// </summary>
        /// <param name="aspectContext">拦截上下文</param>
        /// <returns>异步拦截结果</returns>
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
            object result;

            if (valueTypeInfo.IsTaskWithVoidTaskResult())
            {
                return null;
            }
            else if (valueTypeInfo.IsTaskWithResult())
            {
                // Is there better solution to unwrap ?
                result = (object)(await (dynamic)value);
            }
            else if (valueTypeInfo.IsValueTaskWithResult())
            {
                // Is there better solution to unwrap ?
                result = (object)(await (dynamic)value);
            }
            else if (value is Task)
            {
                return null;
            }
            else if (value is ValueTask)
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
                return await Unwrap(result, resultTypeInfo);
            }

            return result;
        }

        private static bool IsAsyncFromMetaData(MethodInfo method)
        {
            //方法返回值为异步类型
            if (IsAsyncType(method.ReturnType.GetTypeInfo()))
            {
                return true;
            }

            //IsDefined方法说明:指示是否将AsyncAspectAttribute类型的一个或多个特性应用于此成员。(要搜索此成员的继承链以查找属性)
            if (method.IsDefined(typeof(AsyncAspectAttribute), true))
            {
                if (method.ReturnType == typeof(object))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 判断类型是否为Task,Task<>,ValueTask,ValueTask<>
        /// </summary>
        /// <param name="typeInfo">类型</param>
        /// <returns>结果</returns>
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