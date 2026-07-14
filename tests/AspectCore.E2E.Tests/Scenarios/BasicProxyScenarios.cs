using System;
using AspectCore.Configuration;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for the core proxy pipeline: service registration → proxy generation
/// → method invocation. No mocks — real DI container, real proxies.
/// </summary>
[Collection("InterceptorLog")]
public class BasicProxyScenarios
{
    [Fact]
    public void InterfaceProxy_SyncMethod_ReturnsOriginalValue()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        // The proxy must preserve original behavior on methods without interceptors.
        Assert.Equal(7, service.Add(3, 4));
        Assert.Equal(1, service.Subtract(5, 4));
    }

    [Fact]
    public void InterfaceProxy_GenericMethod_ReturnsValue()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        Assert.Equal(42, service.Echo(42));
        Assert.Equal("hello", service.Echo("hello"));
    }

    [Fact]
    public void InterfaceProxy_MethodWithDefaultValue_UsesDefault()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        Assert.Equal("hi!", service.Concat("hi"));
        Assert.Equal("hi?", service.Concat("hi", "?"));
    }

    [Fact]
    public void InterfaceProxy_OutParameter_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        service.GetOutput(5, out var doubled);
        Assert.Equal(10, doubled);
    }

    [Fact]
    public void InterfaceProxy_RefParameter_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        var value = 10;
        service.Increment(ref value);
        Assert.Equal(11, value);
    }

    [Fact]
    public void InterfaceProxy_NullableParameter_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        Assert.Equal("Hello, stranger", service.Greet(null));
        Assert.Equal("Hello, Alice", service.Greet("Alice"));
    }

    [Fact]
    public void InterfaceProxy_ParamsArray_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>();

        Assert.Equal(15, service.Sum(new[] { 1, 2, 3, 4, 5 }));
    }

    [Fact]
    public void InterfaceProxy_IsGenerated_WhenInterceptorConfigured()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        // A proxy is generated only when at least one interceptor is configured.
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                AspectCore.Configuration.Predicates.Implement(typeof(ICalculatorService)));
        });

        // The resolved instance must be a generated proxy, not the original type.
        Assert.IsNotType<CalculatorService>(service);
        Assert.IsAssignableFrom<ICalculatorService>(service);
    }

    [Fact]
    public void ClassProxy_VirtualMethod_ReturnsOriginalValue()
    {
        using var host = new TestHost();
        host.Add<CovariantService>();

        var service = host.Resolve<CovariantService>();

        var result = service.Get();
        Assert.Equal("derived", result.Name);
        Assert.Equal(42, ((DerivedResult)result).Extra);
    }

    [Fact]
    public void ClassProxy_IsGenerated_WhenInterceptorConfigured()
    {
        using var host = new TestHost();
        host.Add<CovariantService>();

        var service = host.Resolve<CovariantService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                AspectCore.Configuration.Predicates.ForService(nameof(CovariantService)));
        });

        Assert.IsNotType<CovariantService>(service);
    }

    [Fact]
    public void InternalClassProxy_NonInterceptedMethod_DoesNotThrow()
    {
        // Regression surface for the MethodAccessException fix (issue #274):
        // calling a non-proxied method on an internal class must work.
        using var host = new TestHost();
        host.Add<IInternalService, InternalService>();

        var service = host.Resolve<IInternalService>();

        Assert.IsNotType<InternalService>(service);
        Assert.Equal("intercepted-internal", service.Intercepted());
        Assert.Equal("original-not-intercepted", service.NotIntercepted());
    }

    [Fact]
    public void PublicClassProxy_NonInterceptedMethod_DoesNotThrow()
    {
        using var host = new TestHost();
        host.Add<IInternalService, PublicInternalService>();

        var service = host.Resolve<IInternalService>();

        Assert.IsNotType<PublicInternalService>(service);
        Assert.Equal("intercepted-internal", service.Intercepted());
        Assert.Equal("original-not-intercepted", service.NotIntercepted());
    }

    [Fact]
    public void Proxy_PreservesOriginalServiceBehavior_OnInterceptedMethod()
    {
        // The interceptor on PlaceOrder only logs — it must not change the return value.
        using var host = new TestHost();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IOrderService>();

        Assert.Equal("Order placed: widget", service.PlaceOrder("widget"));
        Assert.Contains("OrderLoggingInterceptor.Before", InterceptorLog.Entries);
        Assert.Contains("OrderLoggingInterceptor.After", InterceptorLog.Entries);
    }

    [Fact]
    public void Proxy_PreservesOriginalServiceBehavior_OnNonInterceptedMethod()
    {
        using var host = new TestHost();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IOrderService>();

        Assert.Equal("Order cancelled: widget", service.CancelOrder("widget"));
        Assert.DoesNotContain(InterceptorLog.Entries, e => e.StartsWith("OrderLoggingInterceptor"));
    }

    [Fact]
    public void Proxy_ServiceRegisteredViaFactory_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService>(_ => new CalculatorService());

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                AspectCore.Configuration.Predicates.Implement(typeof(ICalculatorService)));
        });

        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(7, service.Add(3, 4));
    }
}
