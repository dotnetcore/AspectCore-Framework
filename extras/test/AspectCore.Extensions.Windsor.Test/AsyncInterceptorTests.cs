using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Windsor;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Xunit;

namespace AspectCoreTest.Windsor
{
    [AttributeUsage(AttributeTargets.Method)]
    public class AsyncIncreamentAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);
            await Task.Delay(100); // 此处模拟一个真.异步方法，用于测试线程上下文切换

            if (context.ReturnValue is Task<int> task)
            {
                var result = await task;
                context.ReturnValue = Task.FromResult(result + 1);
            }
            else if (context.ReturnValue is ValueTask<int> valueTask)
            {
                var result = await valueTask;
                context.ReturnValue = new ValueTask<int>(result + 1);
            }
            else if (context.ReturnValue is int result)
            {
                context.ReturnValue = result + 1;
            }
        }
    }

    public class AsyncService
    {
        [AsyncIncreament]
        public virtual void DonotGet(int num)
        {
        }

        [AsyncIncreament]
        public virtual Task DonotGetAsync(int num)
        {
            return Task.CompletedTask;
        }

        [AsyncIncreament]
        public virtual int Get(int num)
        {
            return num;
        }

        [AsyncIncreament]
        public virtual async Task<int> GetAsyncWithTask(int num)
        {
            await Task.Delay(100);
            return num;
        }

        [AsyncIncreament]
        public virtual async ValueTask<int> GetAsyncWithValueTask(int num)
        {
            await Task.Delay(100);
            return num;
        }
    }

    public class AsyncMethodTests
    {
        public static IEnumerable<object[]> GetNumbers()
        {
            yield return new object[] { 1 };
            yield return new object[] { 10 };
            yield return new object[] { 100 };
        }

        private static IWindsorContainer CreateWindsorContainer()
        {
            return new WindsorContainer()
                .AddAspectCoreFacility()
                .Register(Component.For<AsyncService>().LifestyleTransient());
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public void TestIncreamentForVoid(int input)
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<AsyncService>();
            service.DonotGet(input);
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncreamentForTask(int input)
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<AsyncService>();
            await service.DonotGetAsync(input);
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public void TestIncreamentForResult(int input)
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<AsyncService>();
            Assert.Equal(input + 1, service.Get(input));
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncreamentForTaskResult(int input)
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<AsyncService>();
            Assert.Equal(input + 1, await service.GetAsyncWithTask(input));
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncreamentForValueTaskResult(int input)
        {
            var container = CreateWindsorContainer();
            var service = container.Resolve<AsyncService>();
            Assert.Equal(input + 1, await service.GetAsyncWithValueTask(input));
        }
    }
}
