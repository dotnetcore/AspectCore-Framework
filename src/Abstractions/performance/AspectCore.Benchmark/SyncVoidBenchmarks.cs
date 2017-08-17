using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;

namespace AspectCore.Benchmark
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SyncVoidBenchmarks
    {
        private readonly static IService service = ProxyFactory.CreateProxy<IService>(new Service());
        private readonly static IService realService = new Service();

        [Benchmark]
        public Task Call()
        {
            return realService.Foo();
        }

        [Benchmark]
        public Task AspectCore_Proxy()
        {
            return service.Foo();
        }
    }

    [MyInterceptor]
    public interface IService
    {
        Task<int> Foo();
    }

    public class Service : IService
    {
        public Task<int> Foo()
        {
            return Task.FromResult(1);
        }
    }

    public class MyInterceptor : InterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            //短路后续拦截器 并直接返回结果
            return context.Break();
        }
    }
}
