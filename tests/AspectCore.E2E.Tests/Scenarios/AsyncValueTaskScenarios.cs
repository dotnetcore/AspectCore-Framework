using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for async and ValueTask scenarios: Task/ValueTask return types,
/// async exception handling, async interceptor chains, and async method
/// combinations. Real DI container, real async pipeline — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class AsyncValueTaskScenarios
{
    [Fact]
    public async Task TaskReturn_WithInterceptor_ObservesResult()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Task.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Task.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();
        Assert.Equal("async-name", result);
        Assert.Contains("Task.Before", InterceptorLog.Entries);
        Assert.Contains("Task.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task ValueTaskReturn_WithInterceptor_ObservesResult()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("ValueTask.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("ValueTask.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetLabelAsync();
        Assert.Equal("async-label", result);
        Assert.Contains("ValueTask.Before", InterceptorLog.Entries);
        Assert.Contains("ValueTask.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task TaskOfT_WithMultipleSequentialCalls_AllWork()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Multiple sequential Task<T> calls.
        var r1 = await service.MultiplyAsync(2, 3);
        var r2 = await service.MultiplyAsync(4, 5);
        var r3 = await service.MultiplyAsync(6, 7);

        Assert.Equal(6, r1);
        Assert.Equal(20, r2);
        Assert.Equal(42, r3);
    }

    [Fact]
    public async Task ValueTaskOfT_WithMultipleSequentialCalls_AllWork()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Multiple sequential ValueTask<T> calls.
        var r1 = await service.DivideAsync(10, 2);
        var r2 = await service.DivideAsync(15, 3);
        var r3 = await service.DivideAsync(20, 4);

        Assert.Equal(5, r1);
        Assert.Equal(5, r2);
        Assert.Equal(5, r3);
    }

    [Fact]
    public async Task AsyncMethod_Exception_PropagatesThroughInterceptor()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        var service = host.Resolve<IThrowingService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IThrowingService)));
        });

        // Async exception propagates through the proxy.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DoWorkAsync("fail"));
        Assert.Equal("async failure", ex.Message);
    }

    [Fact]
    public async Task AsyncMethod_Exception_WrappedInAspectException_WhenConfigured()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        var service = host.Resolve<IThrowingService>(config =>
        {
            config.ThrowAspectException = true;
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IThrowingService)));
        });

        // With ThrowAspectException = true, the exception is wrapped.
        var aspectEx = await Assert.ThrowsAsync<AspectInvocationException>(() => service.DoWorkAsync("fail"));
        Assert.NotNull(aspectEx.InnerException);
        Assert.IsType<InvalidOperationException>(aspectEx.InnerException);
    }

    [Fact]
    public async Task AsyncMethod_SwallowedException_ReturnsFallback()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IThrowingService>(config =>
        {
            // Use a custom async-aware interceptor that swallows the exception
            // and sets a fallback Task<string> return value.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                try
                {
                    await ctx.Invoke(next);
                }
                catch (Exception ex)
                {
                    InterceptorLog.Entries.Add($"Swallowed:{ex.GetType().Name}");
                    ctx.ReturnValue = Task.FromResult("swallowed");
                }
            }, Predicates.Implement(typeof(IThrowingService)));
        });

        // The interceptor swallows the exception and returns a fallback value.
        var result = await service.DoWorkAsync("fail");
        Assert.Equal("swallowed", result);
        Assert.Contains("Swallowed:InvalidOperationException", InterceptorLog.Entries);
    }

    [Fact]
    public async Task AsyncMethod_RethrownDifferentException_WrapsException()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IThrowingService>(config =>
        {
            config.Interceptors.AddTyped<RethrowDifferentInterceptorAttribute>(
                Predicates.Implement(typeof(IThrowingService)));
        });

        // The interceptor catches and rethrows with a different exception type.
        var ex = await Assert.ThrowsAsync<ApplicationException>(() => service.DoWorkAsync("fail"));
        Assert.Equal("wrapped by interceptor", ex.Message);
        Assert.Contains("Rethrew:InvalidOperationException", InterceptorLog.Entries);
    }

    [Fact]
    public async Task AsyncMethod_MultipleInterceptors_AllExecuteInOrder()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddTyped<FirstInterceptorAttribute>(
                Predicates.Implement(typeof(IAsyncService)));
            config.Interceptors.AddTyped<SecondInterceptorAttribute>(
                Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();
        Assert.Equal("async-name", result);

        // First runs before Second (lower Order = higher priority).
        Assert.Equal("First.Before", InterceptorLog.Entries[0]);
        Assert.Equal("Second.Before", InterceptorLog.Entries[1]);
        Assert.Equal("Second.After", InterceptorLog.Entries[2]);
        Assert.Equal("First.After", InterceptorLog.Entries[3]);
    }

    [Fact]
    public async Task TaskAndValueTask_MixedCalls_Work()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Mix Task<T> and ValueTask<T> calls.
        var taskResult = await service.MultiplyAsync(3, 4);
        var valueTaskResult = await service.DivideAsync(10, 2);

        Assert.Equal(12, taskResult);
        Assert.Equal(5, valueTaskResult);
    }

    [Fact]
    public async Task AsyncMethod_VoidTask_WithInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("VoidTask.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("VoidTask.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        // Void Task (no return value) with interceptor.
        await service.ChainAsync();
        Assert.Contains("VoidTask.Before", InterceptorLog.Entries);
        Assert.Contains("VoidTask.After", InterceptorLog.Entries);
    }
}
