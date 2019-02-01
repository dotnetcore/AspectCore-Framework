using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
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

        private static IServiceContainer CreateContainer()
        {
            var c = new ServiceContainer();
            c.RegisterDynamicProxy()
             .Register<AsyncService>();
            return c;
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public void TestIncreamentForVoid(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            service.DonotGet(input);
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncreamentForTask(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            await service.DonotGetAsync(input);
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public void TestIncreamentForResult(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            Assert.Equal(input + 1, service.Get(input));
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncreamentForTaskResult(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            Assert.Equal(input + 1, await service.GetAsyncWithTask(input));
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncreamentForValueTaskResult(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            Assert.Equal(input + 1, await service.GetAsyncWithValueTask(input));
        }
    }
}
