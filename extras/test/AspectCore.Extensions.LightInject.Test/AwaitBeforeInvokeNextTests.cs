using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AwaitBeforeInvokeAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await Task.Delay(100);
            if (context.Proxy is AwaitBeforeInvokeTester tester
                && tester.Cts.IsCancellationRequested)
            {
                context.ReturnValue = Task.FromResult(-1);
                return;
            }
            await context.Invoke(next);
        }
    }

    public class AwaitBeforeInvokeTester
    {
        public CancellationTokenSource Cts { get; set; }

        [AwaitBeforeInvoke]
        public virtual async Task<int> ExecuteAsync()
        {
            await Task.Delay(100);
            return 1;
        }
    }

    public class AwaitBeforeInvokeNextTests
    {
        private static IServiceContainer CreateContainer()
        {
            var c = new ServiceContainer(new ContainerOptions
            {
                DefaultServiceSelector = s => s.Last(),
                EnablePropertyInjection = false
            });
            c.RegisterDynamicProxy()
                .Register<AwaitBeforeInvokeTester>();
            return c;
        }

        [Fact]
        public async Task Test()
        {
            var container = CreateContainer();
            var service = container.GetInstance<AwaitBeforeInvokeTester>();
            service.Cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var result = await service.ExecuteAsync();
            Assert.Equal(1, result);
            Assert.False(service.Cts.IsCancellationRequested);
        }
    }
}
