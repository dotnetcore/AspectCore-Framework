using System;
using BenchmarkDotNet.Running;

namespace AspectCore.IoC.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SimpleObjectBenchmarks>();
        }
    }
}
