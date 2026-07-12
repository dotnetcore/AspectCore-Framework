using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Extensions.LightInject;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class AsyncService
    {
        [AsyncIncrement]
        public virtual void DoNotGet(int num)
        {
        }

        [AsyncIncrement]
        public virtual Task DoNotGetAsync(int num)
        {
            return Task.CompletedTask;
        }

        [AsyncIncrement]
        public virtual int Get(int num)
        {
            return num;
        }

        [AsyncIncrement]
        public virtual async Task<int> GetAsyncWithTask(int num)
        {
            await Task.Delay(100);
            return num;
        }

        [AsyncIncrement]
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
        public void TestIncrementForVoid(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            service.DoNotGet(input);
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncrementForTask(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            await service.DoNotGetAsync(input);
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public void TestIncrementForResult(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            Assert.Equal(input + 1, service.Get(input));
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncrementForTaskResult(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            Assert.Equal(input + 1, await service.GetAsyncWithTask(input));
        }

        [Theory]
        [MemberData(nameof(GetNumbers))]
        public async Task TestIncrementForValueTaskResult(int input)
        {
            var container = CreateContainer();
            var service = container.GetInstance<AsyncService>();
            Assert.Equal(input + 1, await service.GetAsyncWithValueTask(input));
        }
    }
}
