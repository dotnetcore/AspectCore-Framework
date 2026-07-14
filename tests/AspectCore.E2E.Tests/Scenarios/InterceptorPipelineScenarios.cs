using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for the interceptor pipeline: execution, ordering, return value
/// modification, short-circuiting, and AllowMultiple behavior. Real interceptors,
/// real DI container — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class InterceptorPipelineScenarios
{
    [Fact]
    public void SingleInterceptor_Executes_BeforeAndAfter()
    {
        using var host = new TestHost();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IOrderService>();

        var result = service.PlaceOrder("widget");

        Assert.Equal("Order placed: widget", result);
        Assert.Contains("OrderLoggingInterceptor.Before", InterceptorLog.Entries);
        Assert.Contains("OrderLoggingInterceptor.After", InterceptorLog.Entries);
    }

    [Fact]
    public void MultipleInterceptors_ExecuteInOrder()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Apply both ordered interceptors to all methods of ICalculatorService.
            config.Interceptors.AddTyped<FirstInterceptorAttribute>(
                Predicates.Implement(typeof(ICalculatorService)));
            config.Interceptors.AddTyped<SecondInterceptorAttribute>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(1, 2);

        Assert.Equal(3, result);

        // First (Order=1) runs before Second (Order=2).
        Assert.Equal("First.Before", InterceptorLog.Entries[0]);
        Assert.Equal("Second.Before", InterceptorLog.Entries[1]);
        Assert.Equal("Second.After", InterceptorLog.Entries[2]);
        Assert.Equal("First.After", InterceptorLog.Entries[3]);
    }

    [Fact]
    public void AllowMultipleFalse_OnlyExecutesOnce()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Register the same interceptor type twice. AllowMultiple = false
            // (the default) means only one instance should execute per method.
            config.Interceptors.AddTyped<OnceInterceptorAttribute>(
                Predicates.Implement(typeof(ICalculatorService)));
            config.Interceptors.AddTyped<OnceInterceptorAttribute>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(1, 1);

        Assert.Equal(2, result);
        Assert.Single(InterceptorLog.Entries, e => e == "Once");
    }

    [Fact]
    public void Interceptor_CanModifyReturnValue()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            // AppendInterceptor modifies the string return value by appending "-modified".
            config.Interceptors.AddTyped(
                typeof(AppendInterceptorAttribute),
                new object[] { "-modified" },
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Concat returns a string, so the interceptor appends "-modified".
        var result = service.Concat("hello");

        Assert.Equal("hello!-modified", result);
    }

    [Fact]
    public void Interceptor_CanShortCircuit_Break()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            // ShortCircuitInterceptor sets ReturnValue and calls Break() —
            // the real implementation is never invoked.
            config.Interceptors.AddTyped<ShortCircuitInterceptorAttribute>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Concat normally returns "hi!", but the short-circuit interceptor
        // replaces it with "short-circuited" without calling the real method.
        var result = service.Concat("hi");

        Assert.Equal("short-circuited", result);
    }

    [Fact]
    public void Interceptor_AttributeAndConfiguration_BothExecute()
    {
        // PlaceOrder has [OrderLoggingInterceptor] attribute; we also add a
        // configuration-based interceptor. Both should execute.
        using var host = new TestHost();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IOrderService>(config =>
        {
            config.Interceptors.AddTyped(
                typeof(AppendInterceptorAttribute),
                new object[] { "!" },
                Predicates.Implement(typeof(IOrderService)));
        });

        var result = service.PlaceOrder("item");

        // The attribute interceptor logs, then the config interceptor appends "!".
        Assert.Contains("OrderLoggingInterceptor.Before", InterceptorLog.Entries);
        Assert.Equal("Order placed: item!", result);
    }

    [Fact]
    public void Interceptor_DelegateInterceptor_Executes()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (context, next) =>
            {
                InterceptorLog.Entries.Add("Delegate.Before");
                await context.Invoke(next);
                InterceptorLog.Entries.Add("Delegate.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(5, 5);

        Assert.Equal(10, result);
        Assert.Contains("Delegate.Before", InterceptorLog.Entries);
        Assert.Contains("Delegate.After", InterceptorLog.Entries);
    }
}
