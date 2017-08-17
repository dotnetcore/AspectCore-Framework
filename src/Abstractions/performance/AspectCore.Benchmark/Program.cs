using System;
using BenchmarkDotNet.Running;

namespace AspectCore.Benchmark
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SyncVoidBenchmarks>();
        }
    }
}
