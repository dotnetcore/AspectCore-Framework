using System.Threading.Tasks;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ParameterAspectInvokerTests
    {
        [Fact]
        public async Task Invoke_WithNoDelegates_ReturnsCompletedTask()
        {
            var invoker = new ParameterAspectInvoker();
            var context = new ParameterAspectContext();
            var task = invoker.Invoke(context);
            await task;
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task Invoke_WithSingleDelegate_InvokesDelegate()
        {
            var invoker = new ParameterAspectInvoker();
            var called = false;
            invoker.AddDelegate((ctx, next) =>
            {
                called = true;
                return next(ctx);
            });
            var context = new ParameterAspectContext();
            await invoker.Invoke(context);
            Assert.True(called);
        }

        [Fact]
        public async Task Invoke_WithMultipleDelegates_InvokesAllInOrder()
        {
            var invoker = new ParameterAspectInvoker();
            var order = "";
            invoker.AddDelegate((ctx, next) =>
            {
                order += "1";
                return next(ctx);
            });
            invoker.AddDelegate((ctx, next) =>
            {
                order += "2";
                return next(ctx);
            });
            invoker.AddDelegate((ctx, next) =>
            {
                order += "3";
                return next(ctx);
            });
            var context = new ParameterAspectContext();
            await invoker.Invoke(context);
            Assert.Equal("123", order);
        }

        [Fact]
        public async Task Invoke_WithDelegateThatDoesNotCallNext_StopsChain()
        {
            var invoker = new ParameterAspectInvoker();
            var secondCalled = false;
            invoker.AddDelegate((ctx, next) =>
            {
                // Don't call next
                return Task.CompletedTask;
            });
            invoker.AddDelegate((ctx, next) =>
            {
                secondCalled = true;
                return next(ctx);
            });
            var context = new ParameterAspectContext();
            await invoker.Invoke(context);
            Assert.False(secondCalled);
        }

        [Fact]
        public async Task Reset_ClearsDelegates()
        {
            var invoker = new ParameterAspectInvoker();
            var called = false;
            invoker.AddDelegate((ctx, next) =>
            {
                called = true;
                return next(ctx);
            });
            invoker.Reset();
            var context = new ParameterAspectContext();
            await invoker.Invoke(context);
            Assert.False(called);
        }

        [Fact]
        public async Task Invoke_AfterReset_StartsFresh()
        {
            var invoker = new ParameterAspectInvoker();
            invoker.AddDelegate((ctx, next) => next(ctx));
            invoker.Reset();

            var called = false;
            invoker.AddDelegate((ctx, next) =>
            {
                called = true;
                return next(ctx);
            });
            var context = new ParameterAspectContext();
            await invoker.Invoke(context);
            Assert.True(called);
        }
    }
}
