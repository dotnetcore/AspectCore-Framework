using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
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

        private static readonly ConcurrentDictionary<TypeInfo, Func<object, object>> _resultFuncCache = new ConcurrentDictionary<TypeInfo, Func<object, object>>();

        private static readonly ConcurrentDictionary<TypeInfo, Func<object, Task>> _asTaskFuncCache = new ConcurrentDictionary<TypeInfo, Func<object, Task>>();

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
                {
                    var type = returnValue.GetType().GetTypeInfo();
                    if (type.IsValueTaskWithResult())
                    {
                        await ValueTaskWithResultToTask(returnValue, type);
                    }
                    break;
                }
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
            return (T)await UnwrapAsyncReturnValue(aspectContext);
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

        // value should be ValueTask<T>
        private static Task ValueTaskWithResultToTask(object value, TypeInfo valueTypeInfo)
        {
            // NOTE: if we use "await (dynamic)value" to await a ValueTask<T> when T is non-public, we will get an RuntimeBinderException that says
            // 'System.ValueType' does not contain a definition for 'GetAwaiter'.
            // So we have to convert ValueTask<T> to Task and then await it.
            // Please fix this logic if there is a better solution.
            var func = _asTaskFuncCache.GetOrAdd(valueTypeInfo, k =>
            {
                var parameter = Expression.Parameter(typeof(object), "type");
                var convertedParameter = Expression.Convert(parameter, valueTypeInfo);
                var method = valueTypeInfo.GetMethod(nameof(ValueTask<int>.AsTask));
                var property = Expression.Call(convertedParameter, method);
                var convertedProperty = Expression.Convert(property, typeof(Task));
                var exp = Expression.Lambda<Func<object, Task>>(convertedProperty, parameter);
                return exp.Compile();
            });
            return func(value);
        }

        // value should be Task<T> or ValueTask<T>
        private static object GetTaskResult(object value, TypeInfo valueTypeInfo)
        {
            // result: if we use "result = (object)(await (dynamic)value)" to get the result of a ValueTask<T> when T is non-public, we will get an RuntimeBinderException that says
            // 'System.ValueType' does not contain a definition for 'GetAwaiter'.
            // So after ValueTask<T> is awaited, we use expression with cache to get the value of ValueTask<T>.Result.
            // The Benchmark can be viewed in GetTaskResultBenchmarks.cs in AspectCore.Extensions.Reflection.Benchmark.
            // Here is a sample result: 
            /*
                |                            Method |         Mean |
                |---------------------------------- |-------------:|
                |          GetTaskResult_Reflection |     342.0 ns |
                | GetTaskResult_ReflectionWithCache |     279.5 ns |
                |          GetTaskResult_Expression | 224,219.4 ns |
                | GetTaskResult_ExpressionWithCache |     125.9 ns |
                |        GetTaskResult_AwaitDynamic |     124.8 ns |
            */
            // Please fix this logic if there is a better solution.
            var func = _resultFuncCache.GetOrAdd(valueTypeInfo, k =>
            {
                var parameter = Expression.Parameter(typeof(object), "type");
                var convertedParameter = Expression.Convert(parameter, valueTypeInfo);
                var property = Expression.Property(convertedParameter, nameof(Task<int>.Result));
                var convertedProperty = Expression.Convert(property, typeof(object));
                var exp = Expression.Lambda<Func<object, object>>(convertedProperty, parameter);
                return exp.Compile();
            });
            return func(value);
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
                await (Task)value;
                result = GetTaskResult(value, valueTypeInfo);
            }
            else if (valueTypeInfo.IsValueTaskWithResult())
            {
                await ValueTaskWithResultToTask(value, valueTypeInfo);
                result = GetTaskResult(value, valueTypeInfo);
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