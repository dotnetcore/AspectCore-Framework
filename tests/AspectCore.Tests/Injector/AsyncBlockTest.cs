using AspectCore.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

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

        [Fact]
        public async Task AsyncBlock()
        {
            var builder = new ProxyGeneratorBuilder();
            builder.Configure(_ => { });
            var proxyGenerator = builder.Build();
            var proxy = proxyGenerator.CreateInterfaceProxy<IService1, Service1>();
            // IService proxy = new Service();
            var startTime = DateTime.Now;
            Console.WriteLine($"{startTime}:start");

            var val = proxy.GetValue("le");

            var endTime = DateTime.Now;

            Assert.True((endTime - startTime).TotalSeconds < 2);
            Console.WriteLine($"{endTime}:should return immediately");
            var result = await val;
            Console.WriteLine($"{DateTime.Now}:{result}");
        }
    }
}
