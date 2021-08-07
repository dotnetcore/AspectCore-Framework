using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Core.Benchmark.Benchmarks
{
    [AllStatisticsColumn]
    [MemoryDiagnoser]
    [StopOnFirstError]
    public class GetTaskResultBenchmarks
    {
        private static readonly (object TaskWithResult, TypeInfo Type, object Result)[] Cases =
            new object[] { 1, "str", null }
                .Select(m => (m, Task.FromResult(m)))
                .Select(m => ((object)m.Item2, m.Item2.GetType().GetTypeInfo(), m.m))
                .ToArray();

        private static readonly ConcurrentDictionary<TypeInfo, PropertyInfo> _propertyInfos = new();
        private static readonly ConcurrentDictionary<TypeInfo, Func<object, object>> _delegates = new();

        private static readonly MethodInfo _methodOfCreateDelegate = typeof(GetTaskResultBenchmarks).GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
            .First(m => m.Name == nameof(CreateDelegateToGetProperty) && m.IsGenericMethod);
        private static readonly ConcurrentDictionary<TypeInfo, Func<object, object>> _dynamicDelegates = new();

        private static readonly ConcurrentDictionary<TypeInfo, Func<object, object>> _dynamicMethods = new();

        private static async Task<object> GetTaskResult_ReflectionWithCache(object value, TypeInfo valueTypeInfo)
        {
            await (Task)value;
            var prop = _propertyInfos.GetOrAdd(valueTypeInfo, k => k.GetProperty(nameof(Task<int>.Result)));
            var result = prop.GetValue(value);
            return result;
        }

        private static async Task<object> GetTaskResult_AwaitDynamic(object value, TypeInfo valueTypeInfo)
        {
            var result = (object)(await (dynamic)value);
            return result;
        }

        private static async Task<object> GetTaskResult_ExpressionWithCache(object value, TypeInfo valueTypeInfo)
        {
            await (Task)value;
            var func = _delegates.GetOrAdd(valueTypeInfo, k => AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(k));
            return func(value);
        }

        private static Func<object, object> CreateDelegateToGetProperty<T, TProperty>(PropertyInfo property)
        {
            var func = (Func<T, TProperty>)Delegate.CreateDelegate(typeof(Func<T, TProperty>), property.GetGetMethod()!);
            return o => func((T)o);
        }

        private static Func<object, object> CreateDelegateToGetProperty(TypeInfo valueTypeInfo, string propertyName)
        {
            var prop = valueTypeInfo.GetProperty(propertyName);
            var func = (Func<PropertyInfo, Func<object, object>>)_methodOfCreateDelegate
                .MakeGenericMethod(valueTypeInfo, prop.PropertyType)
                .CreateDelegate(typeof(Func<PropertyInfo, Func<object, object>>));
            return func(prop);
        }

        private static async Task<object> GetTaskResult_DynamicDelegateWithCache(object value, TypeInfo valueTypeInfo)
        {
            await (Task)value;
            var func = _dynamicDelegates.GetOrAdd(valueTypeInfo, k => CreateDelegateToGetProperty(k, nameof(Task<int>.Result)));
            return func(value);
        }

        private static async Task<object> GetTaskResult_DynamicMethodWithCache(object value, TypeInfo valueTypeInfo)
        {
            await (Task)value;
            var func = _dynamicMethods.GetOrAdd(valueTypeInfo, k => CreateDynamicMethodToGetTaskResult(k));
            return func(value);
        }

        private static Func<object, object> CreateDynamicMethodToGetTaskResult(TypeInfo valueTypeInfo)
        {
            var prop = valueTypeInfo.GetProperty(nameof(Task<int>.Result));

            var method = new DynamicMethod(
                name: "__IL_GetTaskResult<" + valueTypeInfo.Name + ">",
                returnType: prop.PropertyType,
                parameterTypes: new[] { typeof(object) },
                owner: typeof(object),
                skipVisibility: true);

            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Castclass, valueTypeInfo);
            il.Emit(OpCodes.Callvirt, prop.GetGetMethod()!);
            il.Emit(OpCodes.Castclass, typeof(object));
            il.Emit(OpCodes.Ret);
            var func = (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
            return func;
        }

        public static async Task GetTaskResult_Test(Func<object, TypeInfo, Task<object>> func)
        {
            foreach (var @case in Cases)
            {
                var result = await func(@case.TaskWithResult, @case.Type);
                if (!Equals(@case.Result, result))
                {
                    throw new InvalidOperationException();
                }
            }
        }

        [Benchmark]
        public async Task GetTaskResult_ReflectionWithCache()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_ReflectionWithCache(v, t));
        }

        [Benchmark]
        public async Task GetTaskResult_ExpressionWithCache()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_ExpressionWithCache(v, t));
        }

        [Benchmark(Baseline = true)]
        public async Task GetTaskResult_AwaitDynamic()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_AwaitDynamic(v, t));
        }

        [Benchmark]
        public async Task GetTaskResult_DynamicDelegateWithCache()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_DynamicDelegateWithCache(v, t));
        }

        [Benchmark]
        public async Task GetTaskResult_DynamicMethodWithCache()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_DynamicMethodWithCache(v, t));
        }
    }
}
