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

    public class InvokeEndIntercept : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"{startTime}:start");
            var task = next(context);

            if (context.ReturnValue is Task resultTask)
            {
                resultTask.ContinueWith((o) =>
                {
                    // 被代理的方法已经执行
                    var startTimeInner = startTime;
                    var endTime = DateTime.Now;
                    Console.WriteLine($"{endTime}:end");
                });
            }
            else
            {
                var endTime = DateTime.Now;
                Console.WriteLine($"{endTime}:end");
            }
            return task;
        }
    }


    public class InvokeEndFailtIntercept : AbstractInterceptorAttribute
    {
        public async override Task Invoke(AspectContext context, AspectDelegate next)
        {
            var startTime = DateTime.Now;
            Console.WriteLine($"{startTime}:start");
            await next(context);
            //未等待被代理的方法执行
            var endTime = DateTime.Now;
            Console.WriteLine($"{endTime}:end");
        }
    }


    public interface IService1
    {
        Task<string> GetValue(string val);

        Task<string> GetValue2(string val);

        Task<string> GetValue3(string val);


    }

    public class Service1 : IService1
    {
        [Intercept1]
        public async Task<string> GetValue(string val)
        {
            await Task.Delay(3000);
            return val;
        }


        [InvokeEndIntercept]
        public async Task<string> GetValue2(string val)
        {
            await Task.Delay(4000);
            return val;
        }


        [InvokeEndFailtIntercept]
        public async Task<string> GetValue3(string val)
        {
            await Task.Delay(3000);
            return val;
        }
    }

    public class AsyncBlockTest : InjectorTestBase
    {

        [Fact]
        public async void AsyncBlock()
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
            Console.WriteLine($"{DateTime.Now}:{val.Result}");

            var val2 = await proxy.GetValue2("le2");
            Console.WriteLine($"{val2}");


            var val3 = await proxy.GetValue3("le3");
            Console.WriteLine($"{val3}");
        }
    }
}
