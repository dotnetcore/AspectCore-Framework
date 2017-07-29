using System;
using AspectCore.Extensions.Reflection.Benchmark.Benchmarks;
using BenchmarkDotNet.Running;

namespace AspectCore.Extensions.Reflection.Benchmark
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //BenchmarkRunner.Run<ConstructorReflectorBenchmarks>();
            //BenchmarkRunner.Run<FieldReflectorBenchmarks>();
            //BenchmarkRunner.Run<MethodReflectorBenchmarks>();
            BenchmarkRunner.Run<PropertyReflectorBenchmarks>();
            Console.ReadKey();
        }
    }
}
