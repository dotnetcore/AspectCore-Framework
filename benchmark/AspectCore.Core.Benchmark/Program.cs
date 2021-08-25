using AspectCore.Core.Benchmark.Benchmarks;
using BenchmarkDotNet.Running;

namespace AspectCore.Core.Benchmark
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            BenchmarkRunner.Run<GetTaskResultBenchmarks>();
        }
    }
}