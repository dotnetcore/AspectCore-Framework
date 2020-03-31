using System;
using System.Reflection;
using AspectCore.Extensions.Reflection.Benchmark.Fakes;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Extensions.Reflection.Benchmark.Benchmarks
{
    [AllStatisticsColumn]
    [MemoryDiagnoser]
    public class ConstructorReflectorBenchmarks
    {
        private readonly ConstructorInfo _constructorInfo;
        private readonly ConstructorReflector _reflector;

        private readonly static object[] args = new object[0];

        public ConstructorReflectorBenchmarks()
        {
            _constructorInfo = typeof(ConstructorFakes).GetTypeInfo().GetConstructor(new Type[0]);
            _reflector = _constructorInfo.GetReflector();
        }

        [Benchmark]
        public void Reflection()
        {
            _constructorInfo.Invoke(args);
        }

        [Benchmark]
        public void Reflector()
        {
            _reflector.Invoke();
        }

        [Benchmark]
        public void New()
        {
            var result = new ConstructorFakes();
        }
    }
}