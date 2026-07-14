using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for complex interceptor scenarios: parameter modification,
/// parameter validation, timing, nested interceptor calls, DI-aware interceptors,
/// caching interceptors, and multiple interceptors with different scopes.
/// Real interceptors, real DI container, real proxies — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ComplexInterceptorScenarios
{
    [Fact]
    public void Interceptor_ModifiesParameters_BeforeCallingNext()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Interceptor modifies the first parameter before invoking next.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                var parameters = ctx.GetParameters();
                if (parameters.Count >= 2 && parameters[0].Value is int && parameters[1].Value is int)
                {
                    parameters[0].Value = (int)parameters[0].Value! + 10;
                    parameters[1].Value = (int)parameters[1].Value! + 20;
                }
                InterceptorLog.Entries.Add("ModifyParams.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("ModifyParams.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        // Original call: Add(1, 2). Interceptor changes to Add(11, 22).
        var result = service.Add(1, 2);

        Assert.Equal(33, result);
        Assert.Contains("ModifyParams.Before", InterceptorLog.Entries);
        Assert.Contains("ModifyParams.After", InterceptorLog.Entries);
    }

    [Fact]
    public void Interceptor_ValidatesParameters_AndThrows()
    {
        using var host = new TestHost();
        host.Add<IThrowingService, ThrowingService>();

        var service = host.Resolve<IThrowingService>(config =>
        {
            config.Interceptors.AddTyped<ValidationInterceptorAttribute>(
                Predicates.Implement(typeof(IThrowingService)));
        });

        // "invalid" input triggers the validation interceptor to throw.
        var ex = Assert.Throws<ArgumentException>(() => service.DoWork("invalid"));
        Assert.Contains("invalid", ex.Message);

        // Valid input passes through.
        Assert.Equal("worked:ok", service.DoWork("ok"));
    }

    [Fact]
    public void Interceptor_LogsEntryAndExitTiming()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddTyped<TimingInterceptorAttribute>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(3, 4);

        Assert.Equal(7, result);
        Assert.Contains(InterceptorLog.Entries, e => e.StartsWith("Timing.Enter:Add"));
        Assert.Contains(InterceptorLog.Entries, e => e.StartsWith("Timing.Exit:Add"));
        // Verify entry is logged before exit.
        var enterIndex = InterceptorLog.Entries.FindIndex(e => e.StartsWith("Timing.Enter"));
        var exitIndex = InterceptorLog.Entries.FindIndex(e => e.StartsWith("Timing.Exit"));
        Assert.True(enterIndex < exitIndex);
    }

    [Fact]
    public void NestedInterceptorCall_InnerMethodIsAlsoIntercepted()
    {
        using var host = new TestHost();
        host.Add<IInnerService, InnerService>();
        host.Add<INestedCallService, NestedCallService>();

        InterceptorLog.Clear();
        var service = host.Resolve<INestedCallService>(config =>
        {
            // Intercept both services; the Outer method calls the injected
            // IInnerService.Process internally, triggering a nested interceptor call.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add($"Intercepted:{ctx.ServiceMethod.Name}");
                await ctx.Invoke(next);
            }, Predicates.ForNameSpace("AspectCore.E2E.Tests.Fixtures"));
        });

        var result = service.Outer(5);

        // Both Outer and Process (inner) should be intercepted.
        Assert.Contains("Intercepted:Outer", InterceptorLog.Entries);
        Assert.Contains("Intercepted:Process", InterceptorLog.Entries);
        Assert.Equal("outer(inner(5))", result);
    }

    [Fact]
    public void InterceptorWithDI_ResolvesServiceFromContainer()
    {
        using var host = new TestHost();
        host.Services.AddSingleton<IMessageProvider, MessageProvider>();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddTyped<DiAwareInterceptorAttribute>(
                Predicates.ForMethod("Concat"));
        });

        // Concat returns a string, so the DI-aware interceptor appends the
        // resolved provider message to the return value.
        var result = service.Concat("test");

        Assert.Equal("test!:hello-from-provider", result);
        Assert.Contains("DiAware.Resolved:hello-from-provider", InterceptorLog.Entries);
    }

    [Fact]
    public void CachingInterceptor_ReturnsCachedValue_OnSecondCall()
    {
        CachingInterceptorAttribute.ClearCache();
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddTyped<CachingInterceptorAttribute>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // First call: cache miss.
        var result1 = service.Concat("hello");
        Assert.Equal("hello!", result1);
        Assert.Contains("Caching.Miss:Concat:hello", InterceptorLog.Entries);

        // Second call with same parameter: cache hit, returns cached value.
        InterceptorLog.Clear();
        var result2 = service.Concat("hello");
        Assert.Equal("hello!", result2);
        Assert.Contains("Caching.Hit:Concat:hello", InterceptorLog.Entries);

        // Different parameter: cache miss again.
        InterceptorLog.Clear();
        var result3 = service.Concat("world");
        Assert.Equal("world!", result3);
        Assert.Contains("Caching.Miss:Concat:world", InterceptorLog.Entries);
    }

    [Fact]
    public void MultipleInterceptors_WithDifferentScopes_SomeMatchSomeDont()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // First interceptor: only matches the Add method.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("AddOnly.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("AddOnly.After");
            }, Predicates.ForMethod("Add"));

            // Second interceptor: matches all methods of ICalculatorService.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("All.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("All.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        // Call Add — both interceptors should execute.
        var addResult = service.Add(1, 1);
        Assert.Equal(2, addResult);
        Assert.Contains("AddOnly.Before", InterceptorLog.Entries);
        Assert.Contains("All.Before", InterceptorLog.Entries);

        // Call Subtract — only the "All" interceptor should execute.
        InterceptorLog.Clear();
        var subResult = service.Subtract(5, 3);
        Assert.Equal(2, subResult);
        Assert.DoesNotContain(InterceptorLog.Entries, e => e.StartsWith("AddOnly"));
        Assert.Contains("All.Before", InterceptorLog.Entries);
    }

    [Fact]
    public async Task Interceptor_ModifiesParameters_AsyncMethod_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("AsyncModify.Before");
                await ctx.Invoke(next);
                // Modify the async return value after the method completes.
                var unwrapped = await ctx.UnwrapAsyncReturnValue();
                if (unwrapped is string s)
                {
                    ctx.ReturnValue = Task.FromResult(s + "-modified");
                }
                InterceptorLog.Entries.Add("AsyncModify.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();

        Assert.Equal("async-name-modified", result);
        Assert.Contains("AsyncModify.Before", InterceptorLog.Entries);
        Assert.Contains("AsyncModify.After", InterceptorLog.Entries);
    }

    [Fact]
    public void Interceptor_ShortCircuits_WhenParameterMatchesCondition()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Interceptor short-circuits when first parameter is 0.
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                var parameters = ctx.GetParameters();
                if (parameters.Count > 0 && parameters[0].Value is int v && v == 0)
                {
                    ctx.ReturnValue = -1;
                    return ctx.Break();
                }
                return next(ctx);
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        // Parameter 0 → short-circuit, return -1.
        Assert.Equal(-1, service.Add(0, 5));

        // Parameter non-zero → normal execution.
        Assert.Equal(8, service.Add(3, 5));
    }
}
