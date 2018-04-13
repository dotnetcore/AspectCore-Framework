using AspectCore.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace AspectCore.Tests.Injector
{
    public class Intercept1 : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            context.Parameters[0] = "lemon";
            return context.Invoke(next);
        }
    }


    public interface IService1
    {
        Task<string> GetValue(string val);
    }

    public class Service1 : IService1
    {
        [Intercept1]
        public async Task<string> GetValue(string val)
        {
            await Task.Delay(3000);
            return val;
        }
    }

    public class AsyncBlockTest : InjectorTestBase
    {
        private readonly ITestOutputHelper _output;

        public AsyncBlockTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void AsyncBlock()
        {
            var builder = new ProxyGeneratorBuilder();
            builder.Configure(_ => { });
            var proxyGenerator = builder.Build();
            var proxy = proxyGenerator.CreateInterfaceProxy<IService1, Service1>();
            // IService proxy = new Service();
            var startTime = DateTime.Now;
            _output.WriteLine($"{startTime}:start");

            var val = proxy.GetValue("le");

            var endTime = DateTime.Now;

            Assert.True((endTime - startTime).TotalSeconds < 2);
            _output.WriteLine($"{endTime}:should return immediately");
            var result = val.Result;
            var resultTime = DateTime.Now;
            _output.WriteLine($"{resultTime}:{result}");
            Assert.True((resultTime - startTime).TotalSeconds > 2);
        }
    }
}
