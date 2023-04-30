using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class ThrowExceptionDirectlyTester
    {
        [Nothing]
        public virtual int ThrowOfResult() => throw new ArgumentException();
        [Nothing]
        public virtual Task<int> ThrowAsyncOfTaskResult() => throw new ArgumentException();
        [Nothing]
        public virtual ValueTask<int> ThrowAsyncOfValueTask() => throw new ArgumentException();

        [Nothing]
        public virtual Task ThrowInMainTask()
        {
            return Task.Run(() => throw new ArgumentException())
                .ContinueWith(m =>
                {
                    if (m.IsFaulted)
                        throw m.Exception.InnerException;
                });
        }

        [Nothing]
        public virtual Task ThrowInUncontinuedTasks()
        {
            return Task.Run(() => throw new ArgumentException())
                .ContinueWith(m => Console.WriteLine(m.Status), TaskContinuationOptions.OnlyOnRanToCompletion);
        }
        
        [Nothing]
        public virtual Task ThrowInSubTask()
        {
            return Task.Run(() => Task.FromResult(1))
                .ContinueWith(m => throw new ArgumentException());
        }
    }

    public class ThrowExceptionDirectlyTests : DynamicProxyTestBase
    {
        private static void AssertSameThrows(Func<object> action, Func<object> proxyAction)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                try
                {
                    proxyAction();
                }
                catch (Exception exOfProxy)
                {
                    Assert.Equal(ex.GetType(), exOfProxy.GetType());
                    return;
                }
                throw new InvalidOperationException("no exception in proxyAction");
            }
            throw new InvalidOperationException("no exception in action");
        }

        private static async Task AssertSameThrowsAsync(Func<Task> action, Func<Task> proxyAction)
        {
            try
            {
                await action();
            }
            catch (Exception ex)
            {
                try
                {
                    await proxyAction();
                }
                catch (Exception exOfProxy)
                {
                    Assert.Equal(ex.GetType(), exOfProxy.GetType());
                    return;
                }
                throw new InvalidOperationException("no exception in proxyAction");
            }
            throw new InvalidOperationException("no exception in action");
        }

        [Fact]
        public async Task TestThrowException()
        {
            var obj = new ThrowExceptionDirectlyTester();
            var proxy = ProxyGenerator.CreateClassProxy<ThrowExceptionDirectlyTester>();
            Assert.True(proxy.IsProxy());

            AssertSameThrows(
                () => obj.ThrowOfResult(),
                () => proxy.ThrowOfResult());

            await AssertSameThrowsAsync(
                async () => await obj.ThrowAsyncOfTaskResult(),
                async () => await proxy.ThrowAsyncOfTaskResult());

            await AssertSameThrowsAsync(
                async () => await obj.ThrowAsyncOfValueTask(),
                async () => await proxy.ThrowAsyncOfValueTask());
        }

        [Fact]
        public async Task TestThrowInMainTask()
        {
            var obj = new ThrowExceptionDirectlyTester();
            var proxy = ProxyGenerator.CreateClassProxy<ThrowExceptionDirectlyTester>();
            Assert.True(proxy.IsProxy());

            await AssertSameThrowsAsync(
                async () => await obj.ThrowInMainTask(),
                async () => await proxy.ThrowInMainTask());
        }

        [Fact]
        public async Task TestThrowInSubTask()
        {
            var obj = new ThrowExceptionDirectlyTester();
            var proxy = ProxyGenerator.CreateClassProxy<ThrowExceptionDirectlyTester>();
            Assert.True(proxy.IsProxy());

            await AssertSameThrowsAsync(
                async () => await obj.ThrowInSubTask(),
                async () => await proxy.ThrowInSubTask());
        }

        [Fact]
        public async Task TestThrowInUncontinuedTasks()
        {
            var obj = new ThrowExceptionDirectlyTester();
            var proxy = ProxyGenerator.CreateClassProxy<ThrowExceptionDirectlyTester>();
            Assert.True(proxy.IsProxy());

            // todo re-test
            // await AssertSameThrowsAsync(
            //     async () => await obj.ThrowInUncontinuedTasks(),
            //     async () => await proxy.ThrowInUncontinuedTasks());
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            base.Configure(configuration);
        }
    }
}
