using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for error handling in the AOP pipeline: interceptor exceptions,
/// service method exceptions, async exception wrapping, exception swallowing,
/// exception rethrowing with different types, and multi-interceptor exception
/// short-circuiting. Real DI container, real proxies, real interceptors — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ErrorHandlingScenarios
{
    [Fact]
    public void InterceptorThrows_Exception_PropagatesToCaller()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Interceptor throws before calling next — the real method never runs.
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                throw new InvalidOperationException("interceptor boom");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var ex = Assert.Throws<InvalidOperationException>(() => service.Add(1, 2));
        Assert.Equal("interceptor boom", ex.Message);
    }

    [Fact]
    public void ServiceMethodThrows_InterceptorStillRuns_BeforeThrow()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IThrowingService>(config =>
        {
            // Interceptor logs before and after; the service throws between them.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("After");
            }, Predicates.Implement(typeof(IThrowingService)));
        });

        // The service method throws — the "After" log should NOT appear.
        var ex = Assert.Throws<InvalidOperationException>(() => service.DoWork("fail"));
        Assert.Equal("sync failure", ex.Message);
        Assert.Contains("Before", InterceptorLog.Entries);
        Assert.DoesNotContain(InterceptorLog.Entries, e => e == "After");
    }

    [Fact]
    public async Task AsyncMethodThrows_ExceptionPropagates_ThroughProxy()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        var service = host.Resolve<IThrowingService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IThrowingService)));
        });

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.DoWorkAsync("fail"));
        Assert.Equal("async failure", ex.Message);
    }

    [Fact]
    public async Task AsyncMethodThrows_ExceptionWrapped_WhenThrowAspectException()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        var service = host.Resolve<IThrowingService>(config =>
        {
            config.ThrowAspectException = true;
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IThrowingService)));
        });

        var aspectEx = await Assert.ThrowsAsync<AspectInvocationException>(() => service.DoWorkAsync("fail"));
        Assert.NotNull(aspectEx.InnerException);
        Assert.IsType<InvalidOperationException>(aspectEx.InnerException);
        Assert.Equal("async failure", aspectEx.InnerException.Message);
    }

    [Fact]
    public void InterceptorCatchesAndSwallows_Exception_DoesNotPropagate()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IThrowingService>(config =>
        {
            config.Interceptors.AddTyped<SwallowExceptionInterceptorAttribute>(
                Predicates.Implement(typeof(IThrowingService)));
        });

        // The service throws, but the interceptor swallows it and returns "swallowed".
        var result = service.DoWork("fail");

        Assert.Equal("swallowed", result);
        Assert.Contains("Swallowed:InvalidOperationException", InterceptorLog.Entries);
    }

    [Fact]
    public void InterceptorCatchesAndRethrows_WithDifferentType()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IThrowingService>(config =>
        {
            config.Interceptors.AddTyped<RethrowDifferentInterceptorAttribute>(
                Predicates.Implement(typeof(IThrowingService)));
        });

        // The service throws InvalidOperationException, but the interceptor
        // rethrows it as ApplicationException.
        var ex = Assert.Throws<ApplicationException>(() => service.DoWork("fail"));
        Assert.Equal("wrapped by interceptor", ex.Message);
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Contains("Rethrew:InvalidOperationException", InterceptorLog.Entries);
    }

    [Fact]
    public void MultipleInterceptors_ExceptionInFirst_PreventsRestFromRunning()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // First interceptor throws — second should never execute.
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                InterceptorLog.Entries.Add("First.Run");
                throw new Exception("first failed");
            }, Predicates.Implement(typeof(ICalculatorService)));

            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Second.Run");
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        Assert.Throws<Exception>(() => service.Add(1, 2));

        // Only the first interceptor ran; the second never got a chance.
        Assert.Contains("First.Run", InterceptorLog.Entries);
        Assert.DoesNotContain(InterceptorLog.Entries, e => e == "Second.Run");
    }

    [Fact]
    public async Task AsyncMethod_ThrowImmediately_PropagatesThroughProxy()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        var service = host.Resolve<IThrowingService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IThrowingService)));
        });

        // ThrowAsyncImmediately throws synchronously inside an async method.
        var ex = await Assert.ThrowsAsync<TimeoutException>(() => service.ThrowAsyncImmediately());
        Assert.Equal("timed out", ex.Message);
    }
}
