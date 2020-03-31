using System;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using Castle.Core;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
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
        [DoNotWire]
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
        private IWindsorContainer CreateWindsorContainer()
        {
            return new WindsorContainer()
                .AddAspectCoreFacility()
                .Register(Component.For<AwaitBeforeInvokeTester>().LifestyleTransient());
        }

        [Fact]
        public async Task Test()
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<AwaitBeforeInvokeTester>();
            service.Cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var result = await service.ExecuteAsync();
            Assert.Equal(1, result);
            Assert.False(service.Cts.IsCancellationRequested);
        }
    }
}
