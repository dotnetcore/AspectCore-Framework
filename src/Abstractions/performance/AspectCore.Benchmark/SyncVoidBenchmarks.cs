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
            service.Foo(0);
        }
    }

    [MyInterceptor]
    public interface IService
    {
        void Foo(int v);
    }

    public class Service : IService
    {
        public void Foo(int v)
        {
        }
    }

    public class MyInterceptor : AspectCore.Abstractions.InterceptorAttribute
    {

    }
}
