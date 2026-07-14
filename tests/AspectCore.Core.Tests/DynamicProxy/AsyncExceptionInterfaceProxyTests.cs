using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AsyncExceptionInterfaceProxyTests : DynamicProxyTestBase
    {
        [Fact]
        public async Task InterfaceProxy_Propagates_Async_Exceptions_With_AspectWrapping()
        {
            var implementation = new AsyncThrowingService();
            var proxy = ProxyGenerator.CreateInterfaceProxy<IAsyncThrowingService, AsyncThrowingService>();

            await AssertWrappedAsyncException(
                () => implementation.ThrowAsyncOfTaskResult(),
                () => proxy.ThrowAsyncOfTaskResult());

            await AssertWrappedAsyncException(
                () => implementation.ThrowAsyncOfValueTask().AsTask(),
                () => proxy.ThrowAsyncOfValueTask().AsTask());

            await AssertWrappedAsyncException(
                () => implementation.ThrowInMainTask(),
                () => proxy.ThrowInMainTask());
        }

        private static async Task AssertWrappedAsyncException(Func<Task> action, Func<Task> proxyAction)
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
                catch (Exception proxyException)
                {
                    Assert.IsType<AspectInvocationException>(proxyException);
                    Assert.NotNull(proxyException.InnerException);
                    Assert.Equal(ex.GetType(), proxyException.InnerException.GetType());
                    return;
                }

                throw new InvalidOperationException("no exception in proxyAction");
            }

            throw new InvalidOperationException("no exception in action");
        }

        public interface IAsyncThrowingService
        {
            [Nothing]
            Task<int> ThrowAsyncOfTaskResult();

            [Nothing]
            ValueTask<int> ThrowAsyncOfValueTask();

            [Nothing]
            Task ThrowInMainTask();
        }

        public class AsyncThrowingService : IAsyncThrowingService
        {
            public Task<int> ThrowAsyncOfTaskResult() => throw new ArgumentException();

            public ValueTask<int> ThrowAsyncOfValueTask() => throw new ArgumentException();

            public Task ThrowInMainTask()
            {
                return Task.Run(() => throw new ArgumentException())
                    .ContinueWith(task =>
                    {
                        if (task.IsFaulted)
                        {
                            throw task.Exception.InnerException;
                        }
                    });
            }
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
            configuration.ThrowAspectException = true;
            base.Configure(configuration);
        }
    }
}
