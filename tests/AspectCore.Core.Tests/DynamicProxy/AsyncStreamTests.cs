using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AsyncStreamTests : DynamicProxyTestBase
    {
        [Fact]
        public async Task AsyncEnumerable_IsIntercepted_WhenEnumerated()
        {
            var calls = 0;
            var proxyGenerator = new ProxyGeneratorBuilder()
                .Configure(config => config.Interceptors.AddDelegate(async (context, next) =>
                {
                    calls++;
                    await context.Invoke(next);
                }))
                .Build();
            var proxy = proxyGenerator.CreateClassProxy<AsyncStreamService>();

            var values = new List<int>();
            await foreach (var value in proxy.GetValues())
            {
                values.Add(value);
            }

            Assert.Equal(new[] { 1, 2 }, values);
            Assert.Equal(1, calls);
        }

        [Fact]
        public async Task AsyncEnumerable_Propagates_Exceptions_During_Enumeration()
        {
            var proxy = ProxyGenerator.CreateClassProxy<AsyncStreamService>();

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await foreach (var _ in proxy.GetValuesAndThrow())
                {
                }
            });

            Assert.Equal(AsyncStreamService.ExceptionMessage, exception.Message);
        }

        [Fact]
        public async Task IAsyncDisposable_DisposeAsync_IsIntercepted()
        {
            var calls = 0;
            var proxyGenerator = new ProxyGeneratorBuilder()
                .Configure(config => config.Interceptors.AddDelegate(async (context, next) =>
                {
                    calls++;
                    await context.Invoke(next);
                }))
                .Build();
            var proxy = proxyGenerator.CreateClassProxy<AsyncDisposableService>();

            await proxy.DisposeAsync();

            Assert.True(proxy.Disposed);
            Assert.Equal(1, calls);
        }

        public class AsyncStreamService
        {
            public const string ExceptionMessage = "async stream failure";

            public virtual async IAsyncEnumerable<int> GetValues()
            {
                await Task.Yield();
                yield return 1;
                yield return 2;
            }

            public virtual async IAsyncEnumerable<int> GetValuesAndThrow()
            {
                await Task.Yield();
                yield return 1;
                throw new InvalidOperationException(ExceptionMessage);
            }
        }

        public class AsyncDisposableService : IAsyncDisposable
        {
            public bool Disposed { get; private set; }

            public virtual ValueTask DisposeAsync()
            {
                Disposed = true;
                return ValueTask.CompletedTask;
            }
        }
    }
}
