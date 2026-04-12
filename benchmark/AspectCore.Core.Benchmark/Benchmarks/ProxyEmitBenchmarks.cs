using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace AspectCore.Core.Benchmark.Benchmarks
{
    [AllStatisticsColumn]
    [MemoryDiagnoser]
    [StopOnFirstError]
    [ShortRunJob]
    public class ProxyEmitBenchmarks
    {
        [Benchmark]
        public Type CreateInterfaceProxyType_WithoutImplementation()
        {
            return CreateTypeGenerator().CreateInterfaceProxyType(typeof(IProxyEmitBenchmarkService));
        }

        [Benchmark]
        public Type CreateInterfaceProxyType_WithImplementation()
        {
            return CreateTypeGenerator().CreateInterfaceProxyType(typeof(IProxyEmitBenchmarkService), typeof(ProxyEmitBenchmarkService));
        }

        [Benchmark(Baseline = true)]
        public Type CreateClassProxyType()
        {
            return CreateTypeGenerator().CreateClassProxyType(typeof(ProxyEmitBenchmarkService), typeof(ProxyEmitBenchmarkService));
        }

        private static ProxyTypeGenerator CreateTypeGenerator()
        {
            var configuration = new AspectConfiguration();
            configuration.Interceptors.AddDelegate((ctx, next) => next(ctx));
            return new ProxyTypeGenerator(new AspectValidatorBuilder(configuration));
        }

        public interface IProxyEmitBenchmarkService
        {
            string Name { get; set; }

            string Echo(string value);
        }

        public class ProxyEmitBenchmarkService : IProxyEmitBenchmarkService
        {
            public virtual string Name { get; set; }

            public virtual string Echo(string value)
            {
                return value;
            }
        }
    }
}
