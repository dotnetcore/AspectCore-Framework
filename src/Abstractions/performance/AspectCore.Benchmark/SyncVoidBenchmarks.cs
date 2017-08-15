using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;

namespace AspectCore.Benchmark
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SyncVoidBenchmarks
    {
        private readonly static IService service = ProxyFactory.CreateProxy<IService>(new Service());

        [Benchmark]
        public void Call()
        {
            service.Foo();
        }
    }

    [MyInterceptor]
    public interface IService
    {
        void Foo();
    }

    public class Service : IService
    {
        public void Foo()
        {
        }
    }

    public class MyInterceptor : AspectCore.Abstractions.InterceptorAttribute
    {

    }
}
