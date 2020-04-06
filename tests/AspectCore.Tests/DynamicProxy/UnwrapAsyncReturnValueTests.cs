using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    internal static class Extensions
    {
        public static Task<T> WrapIntoTask<T>(this T obj) => Task.FromResult(obj);
        public static ValueTask<T> WrapIntoValueTask<T>(this T obj) => new ValueTask<T>(obj);
    }

    public class UnwrapAsyncReturnValueTests : DynamicProxyTestBase
    {
        public class Interceptor : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                if (context.Proxy is Service service)
                {
                    service.CurrentValue = null;
                    await context.Invoke(next);
                    var value = await context.UnwrapAsyncReturnValue();
                    service.CurrentValue = value;
                }
                else
                {
                    await context.Invoke(next);
                }
            }
        }

        public static ValueTask CreateCompletedValueTask() => new ValueTask(Task.CompletedTask);
        public static Task CompletedTask => Task.CompletedTask;

        public class Service
        {
            public object CurrentValue { get; set; }

            [Interceptor] public virtual Task Task() => CompletedTask;
            [Interceptor] public virtual Task<int> TaskT() => 1.WrapIntoTask();
            [Interceptor] public virtual Task<Task> Task_Task() => CompletedTask.WrapIntoTask();
            [Interceptor] public virtual Task<ValueTask> Task_ValueTask() => CreateCompletedValueTask().WrapIntoTask();
            [Interceptor] public virtual Task<Task<int>> Task_TaskT() => 1.WrapIntoTask().WrapIntoTask();
            [Interceptor] public virtual Task<ValueTask<int>> Task_ValueTaskT() => 1.WrapIntoValueTask().WrapIntoTask();

            [Interceptor] public virtual ValueTask ValueTask() => CreateCompletedValueTask();
            [Interceptor] public virtual ValueTask<int> ValueTaskT() => 1.WrapIntoValueTask();
            [Interceptor] public virtual ValueTask<Task> ValueTask_Task() => CompletedTask.WrapIntoValueTask();
            [Interceptor] public virtual ValueTask<ValueTask> ValueTask_ValueTask() => CreateCompletedValueTask().WrapIntoValueTask();
            [Interceptor] public virtual ValueTask<Task<int>> ValueTask_TaskT() => 1.WrapIntoTask().WrapIntoValueTask();
            [Interceptor] public virtual ValueTask<ValueTask<int>> ValueTask_ValueTaskT() => 1.WrapIntoValueTask().WrapIntoValueTask();
        }

        [Fact]
        public async Task Task_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.Task();
            Assert.Null(service.CurrentValue);
        }

        [Fact]
        public async Task TaskT_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.TaskT();
            Assert.Equal(1, service.CurrentValue);
        }

        [Fact]
        public async Task Task_Task_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.Task_Task();
            Assert.Null(service.CurrentValue);
        }

        [Fact]
        public async Task Task_ValueTask_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.Task_ValueTask();
            Assert.Null(service.CurrentValue);
        }

        [Fact]
        public async Task Task_TaskT_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.Task_TaskT();
            Assert.Equal(1, service.CurrentValue);
        }

        [Fact]
        public async Task Task_ValueTaskT_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.Task_ValueTaskT();
            Assert.Equal(1, service.CurrentValue);
        }

        [Fact]
        public async Task ValueTask_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.ValueTask();
            Assert.Null(service.CurrentValue);
        }

        [Fact]
        public async Task ValueTaskT_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.ValueTaskT();
            Assert.Equal(1, service.CurrentValue);
        }

        [Fact]
        public async Task ValueTask_Task_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.ValueTask_Task();
            Assert.Null(service.CurrentValue);
        }

        [Fact]
        public async Task ValueTask_ValueTask_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.ValueTask_ValueTask();
            Assert.Null(service.CurrentValue);
        }

        [Fact]
        public async Task ValueTask_TaskT_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.ValueTask_TaskT();
            Assert.Equal(1, service.CurrentValue);
        }

        [Fact]
        public async Task ValueTask_ValueTaskT_Test()
        {
            var service = ProxyGenerator.CreateClassProxy<Service>();
            await service.ValueTask_ValueTaskT();
            Assert.Equal(1, service.CurrentValue);
        }
    }
}
