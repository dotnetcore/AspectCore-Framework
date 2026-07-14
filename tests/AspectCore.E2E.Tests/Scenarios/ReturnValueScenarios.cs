using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for return value handling: covariant returns, async unwrapping,
/// ValueTask<T>, exception wrapping, invalid casts, null returns, and void
/// methods. All tests run through the real proxy pipeline — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ReturnValueScenarios
{
    [Fact]
    public void CovariantReturnType_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<CovariantService>();

        var service = host.Resolve<CovariantService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(CovariantService)));
        });

        // The method returns DerivedResult, but the base declares BaseResult.
        var result = service.Get();

        Assert.Equal("derived", result.Name);
        Assert.IsType<DerivedResult>(result);
        Assert.Equal(42, ((DerivedResult)result).Extra);
    }

    [Fact]
    public async Task AsyncReturnValue_Unwrapping_TaskOfT_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Unwrap.Before");
                await ctx.Invoke(next);
                // UnwrapAsyncReturnValue extracts the T from Task<T>.
                var unwrapped = await ctx.UnwrapAsyncReturnValue();
                InterceptorLog.Entries.Add($"Unwrap.Result={unwrapped}");
                InterceptorLog.Entries.Add("Unwrap.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();

        Assert.Equal("async-name", result);
        Assert.Contains("Unwrap.Before", InterceptorLog.Entries);
        Assert.Contains("Unwrap.Result=async-name", InterceptorLog.Entries);
        Assert.Contains("Unwrap.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task ValueTaskReturnValue_ThroughProxy_Works()
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
                var unwrapped = await ctx.UnwrapAsyncReturnValue();
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
    public async Task AsyncMethod_ExceptionWrapped_AsAspectInvocationException()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.ThrowAspectException = true;
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        // With ThrowAspectException = true, the original exception is wrapped
        // in AspectInvocationException.
        var ex = await Assert.ThrowsAsync<AspectInvocationException>(() => service.ThrowAsync());
        Assert.NotNull(ex.InnerException);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
        Assert.Equal("boom", ex.InnerException.Message);
    }

    [Fact]
    public async Task AsyncMethod_InvalidCast_ThrowsAspectInvalidCastException()
    {
        using var host = new TestHost();
        host.Add<IReturnValueService, ReturnValueService>();

        var service = host.Resolve<IReturnValueService>(config =>
        {
            // An interceptor that short-circuits and sets ReturnValue to a
            // plain string — NOT a Task<string>. The activator expects a
            // Task<string> return value, so this triggers AspectInvalidCastException.
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                ctx.ReturnValue = "not-a-task";
                return ctx.Break();
            }, Predicates.Implement(typeof(IReturnValueService)));
        });

        var ex = await Assert.ThrowsAsync<AspectInvalidCastException>(async () =>
        {
            await service.GetStringAsync();
        });
        Assert.Contains("Unable to cast", ex.Message);
    }

    [Fact]
    public void NullReturnValue_ReferenceType_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IReturnValueService, ReturnValueService>();

        var service = host.Resolve<IReturnValueService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReturnValueService)));
        });

        var result = service.GetNullString();

        Assert.Null(result);
    }

    [Fact]
    public void NullReturnValue_NullableValueType_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IReturnValueService, ReturnValueService>();

        var service = host.Resolve<IReturnValueService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReturnValueService)));
        });

        var result = service.GetNullableInt();

        Assert.Null(result);
    }

    [Fact]
    public void VoidMethod_WithInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<IReturnValueService, ReturnValueService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IReturnValueService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Void.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Void.After");
            }, Predicates.Implement(typeof(IReturnValueService)));
        });

        // Void method with a side effect (sets a field) plus interceptor logging.
        service.DoWork();

        Assert.True(service.WorkDone);
        Assert.Contains("Void.Before", InterceptorLog.Entries);
        Assert.Contains("Void.After", InterceptorLog.Entries);
    }

    [Fact]
    public void Interceptor_CanReplaceReturnValue_WithNull()
    {
        using var host = new TestHost();
        host.Add<IReturnValueService, ReturnValueService>();

        var service = host.Resolve<IReturnValueService>(config =>
        {
            // Interceptor replaces the return value with null.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                ctx.ReturnValue = null;
            }, Predicates.Implement(typeof(IReturnValueService)));
        });

        // GetString normally returns "hello", but the interceptor sets it to null.
        var result = service.GetString();

        Assert.Null(result);
    }

    /// <summary>
    /// Service interface for return value handling scenarios.
    /// </summary>
    public interface IReturnValueService
    {
        Task<string> GetStringAsync();
        string GetString();
        string? GetNullString();
        int? GetNullableInt();
        void DoWork();
        bool WorkDone { get; }
    }

    /// <summary>
    /// Real implementation of IReturnValueService — no mocks.
    /// </summary>
    public class ReturnValueService : IReturnValueService
    {
        public bool WorkDone { get; private set; }

        public Task<string> GetStringAsync() => Task.FromResult("async-result");

        public string GetString() => "hello";

        public string? GetNullString() => null;

        public int? GetNullableInt() => null;

        public void DoWork()
        {
            WorkDone = true;
        }
    }
}
