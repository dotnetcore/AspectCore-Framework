using System;
using AspectCore.Extensions.Reflection.Benchmark.Benchmarks;
using BenchmarkDotNet.Running;

namespace AspectCore.Extensions.Reflection.Benchmark
{
    static class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ConstructorReflectorBenchmarks>();
            BenchmarkRunner.Run<FieldReflectorBenchmarks>();
            BenchmarkRunner.Run<MethodReflectorBenchmarks>();
            BenchmarkRunner.Run<PropertyReflectorBenchmarks>();
            BenchmarkRunner.Run<CustomAttributeReflectorBenchmarks>();
        }
    }
}