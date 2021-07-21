using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

        private static async Task<object> GetTaskResult_Reflection(object value, TypeInfo valueTypeInfo)
        {
            await (Task)value;
            var result = valueTypeInfo.GetProperty(nameof(Task<int>.Result)).GetValue(value);
            return result;
        }

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

        private static async Task<object> GetTaskResult_Expression(object value, TypeInfo valueTypeInfo)
        {
            await (Task)value;
            var func = AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(valueTypeInfo);
            return func(value);
        }

        private static async Task<object> GetTaskResult_ExpressionWithCache(object value, TypeInfo valueTypeInfo)
        {
            await (Task)value;
            var func = _delegates.GetOrAdd(valueTypeInfo, k => AspectContextRuntimeExtensions.CreateFuncToGetTaskResult(k));
            return func(value);
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
        public async Task GetTaskResult_Reflection()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_Reflection(v, t));
        }

        [Benchmark]
        public async Task GetTaskResult_ReflectionWithCache()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_ReflectionWithCache(v, t));
        }

        [Benchmark]
        public async Task GetTaskResult_Expression()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_Expression(v, t));
        }

        [Benchmark]
        public async Task GetTaskResult_ExpressionWithCache()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_ExpressionWithCache(v, t));
        }

        [Benchmark]
        public async Task GetTaskResult_AwaitDynamic()
        {
            await GetTaskResult_Test((v, t) => GetTaskResult_AwaitDynamic(v, t));
        }
    }
}
