using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        private IWindsorContainer CreateWindsorContainer()
        {
            return new WindsorContainer()
                .AddAspectCoreFacility()
                .Register(Component.For<ExcuteTimesTester>().LifestyleTransient());
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public void Test(int times)
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<ExcuteTimesTester>();
            service.Foo(times);
            Assert.Equal(times, service.ExcuteTimesOfFoo);
        }

        [Theory]
        [MemberData(nameof(Numbers))]
        public async Task TestAsync(int times)
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<ExcuteTimesTester>();
            await service.FooAsync(times);
            Assert.Equal(times, service.ExcuteTimesOfFooAsync);
        }
    }
}
