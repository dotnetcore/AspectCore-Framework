using System;
using System.Threading.Tasks;
using AspectCore.Abstractions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Attributes.Columns;
using Castle.DynamicProxy;

namespace AspectCore.Benchmark
{
    [MemoryDiagnoser]
    [AllStatisticsColumn]
    public class SyncVoidBenchmarks
    {
        private readonly static IService aspectCoreService = ProxyFactory.CreateProxy<IService>(new Service());
        private readonly static IService realService = new Service();
        private readonly static IService castleService = CreateProxtFromCastle();


        static IService CreateProxtFromCastle()
        {
            Castle.DynamicProxy.ProxyGenerator proxyGenerator = new Castle.DynamicProxy.ProxyGenerator();
            return proxyGenerator.CreateInterfaceProxyWithTarget(realService, new MyCastleInterceptor());
        }

        [Benchmark]
        public string DirectCall()
        {
            return realService.GetMessage("DirectCall");
        }

        [Benchmark]
        public string Castle_Proxy()
        {
            return castleService.GetMessage("Castle");
        }

        [Benchmark]
        public string AspectCore_Proxy()
        {
            return aspectCoreService.GetMessage("AspectCore");
        }
    }

    [MyAspectCoreInterceptor]
    public interface IService
    {
        string GetMessage(string libName);
    }

    public class Service : IService
    {
        public string GetMessage(string libName)
        {
            return string.Format("{0}'s aop benchmark {1}.", libName, DateTime.Now);
        }
    }

    public class MyAspectCoreInterceptor : InterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            return next(context);
        }
    }

    public class MyCastleInterceptor : Castle.DynamicProxy.IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
        }
    }
}
