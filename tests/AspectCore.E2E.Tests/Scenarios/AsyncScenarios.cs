using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for the async AOP pipeline: Task<T>, ValueTask<T>, async/await
/// chains, exception propagation, and sequential async calls. Real DI container,
/// real proxies, real interceptors — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class AsyncScenarios
{
    [Fact]
    public async Task TaskReturn_Interception_ObservesResult()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (context, next) =>
            {
                InterceptorLog.Entries.Add("Async.Before");
                await context.Invoke(next);
                var unwrapped = await context.UnwrapAsyncReturnValue();
                InterceptorLog.Entries.Add($"Async.Result={unwrapped}");
                InterceptorLog.Entries.Add("Async.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();

        Assert.Equal("async-name", result);
        Assert.Contains("Async.Before", InterceptorLog.Entries);
        Assert.Contains("Async.Result=async-name", InterceptorLog.Entries);
        Assert.Contains("Async.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task ValueTaskReturn_Interception_ObservesResult()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (context, next) =>
            {
                InterceptorLog.Entries.Add("ValueTask.Before");
                await context.Invoke(next);
                var unwrapped = await context.UnwrapAsyncReturnValue();
                InterceptorLog.Entries.Add($"ValueTask.Result={unwrapped}");
                InterceptorLog.Entries.Add("ValueTask.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetLabelAsync();

        Assert.Equal("async-label", result);
        Assert.Contains("ValueTask.Before", InterceptorLog.Entries);
        Assert.Contains("ValueTask.Result=async-label", InterceptorLog.Entries);
        Assert.Contains("ValueTask.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task AsyncAwait_FullChain_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (context, next) =>
            {
                InterceptorLog.Entries.Add("Chain.Before");
                await context.Invoke(next);
                InterceptorLog.Entries.Add("Chain.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        // Full async/await chain: await the service method from an async test.
        await service.ChainAsync();

        Assert.Contains("Chain.Before", InterceptorLog.Entries);
        Assert.Contains("Chain.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task AsyncMethod_Exception_IsPropagated()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            // Configure an interceptor so a proxy is generated; the exception
            // must propagate through the proxy to the caller.
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        // Without ThrowAspectException, the original exception propagates
        // through the proxy to the caller.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ThrowAsync());
        Assert.Equal("boom", ex.Message);
    }

    [Fact]
    public async Task AsyncMethod_Exception_IsWrapped_WhenConfigured()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.ThrowAspectException = true;
            // A proxy is only generated when at least one interceptor is configured.
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        // With ThrowAspectException = true, the exception is wrapped in
        // AspectInvocationException.
        var aspectEx = await Assert.ThrowsAsync<AspectInvocationException>(() => service.ThrowAsync());
        Assert.NotNull(aspectEx.InnerException);
        Assert.IsType<InvalidOperationException>(aspectEx.InnerException);
    }

    [Fact]
    public async Task AsyncInterceptor_CapturesAndRethrowsException()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddTyped<AsyncExceptionInterceptorAttribute>(
                Predicates.Implement(typeof(IAsyncService)));
        });

        // The interceptor captures the exception, logs it, and rethrows.
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ThrowAsync());
        Assert.Equal("boom", ex.Message);
        Assert.Contains("Caught:InvalidOperationException", InterceptorLog.Entries);
    }

    [Fact]
    public async Task MultipleSequentialAsyncCalls_Work()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>();

        // Multiple sequential async calls through the same proxy instance.
        var name = await service.GetNameAsync();
        var label = await service.GetLabelAsync();
        await service.ChainAsync();
        var name2 = await service.GetNameAsync();

        Assert.Equal("async-name", name);
        Assert.Equal("async-label", label);
        Assert.Equal("async-name", name2);
    }

    [Fact]
    public async Task TaskReturn_OnCalculatorService_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        var product = await service.MultiplyAsync(3, 4);
        Assert.Equal(12, product);

        var quotient = await service.DivideAsync(10, 2);
        Assert.Equal(5, quotient);
    }
}
