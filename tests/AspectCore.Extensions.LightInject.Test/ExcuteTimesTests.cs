using System;
using System.Collections.Generic;
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
    public class ExcuteMultiTimesAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            var para = context.Parameters?.FirstOrDefault();
            if (para is int num)
            {
                for (var i = 0; i < num - 1; i++)
                {
                    await context.Invoke(next);
                }
            }
        }
    }

    public class ExcuteTimesTester
    {
        private int _excuteTimesOfFoo;
        public int ExcuteTimesOfFoo => _excuteTimesOfFoo;

        private int _excuteTimesOfFooAsync;
        public int ExcuteTimesOfFooAsync => _excuteTimesOfFooAsync;

        [ExcuteMultiTimes]
        public virtual void Foo(int times)
        {
            Interlocked.Increment(ref _excuteTimesOfFoo);
        }

        [ExcuteMultiTimes]
        public virtual Task FooAsync(int times)
        {
            Interlocked.Increment(ref _excuteTimesOfFooAsync);
            return Task.CompletedTask;
        }
    }

    public class ExcuteTimesTests
    {
        public static IEnumerable<object[]> Numbers { get; }
            = new[] { 1, 10, 100 }.Select(m => new object[] { m });

        private static IServiceContainer CreateContainer()
        {
            var c = new ServiceContainer(new ContainerOptions
            {
                DefaultServiceSelector = s => s.Last(),
                EnablePropertyInjection = false
            });
            c.RegisterDynamicProxy()
                .Register<ExcuteTimesTester>();
            return c;
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public void Test(int times)
        {
            var container = CreateContainer();
            var service = container.GetInstance<ExcuteTimesTester>();
            service.Foo(times);
            Assert.Equal(times, service.ExcuteTimesOfFoo);
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public async Task TestAsync(int times)
        {
            var container = CreateContainer();
            var service = container.GetInstance<ExcuteTimesTester>();
            await service.FooAsync(times);
            Assert.Equal(times, service.ExcuteTimesOfFooAsync);
        }
    }
}
