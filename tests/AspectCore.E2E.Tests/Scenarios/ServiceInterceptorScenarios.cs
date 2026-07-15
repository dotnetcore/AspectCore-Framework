using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for service interceptors: interceptors registered in the DI container
/// and resolved at runtime via ServiceInterceptorFactory/ServiceInterceptorSelector.
/// Real DI container, real interceptor resolution — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ServiceInterceptorScenarios
{
    [Fact]
    public void ServiceInterceptor_ResolvedFromContainer_IsInvoked()
    {
        using var host = new TestHost();
        // Register the interceptor in the DI container.
        host.Services.AddSingleton<LoggingServiceInterceptor>();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Register the service interceptor via AddServiced.
            config.Interceptors.AddServiced<LoggingServiceInterceptor>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(3, 4);
        Assert.Equal(7, result);
        Assert.Contains("ServiceInterceptor.Before", InterceptorLog.Entries);
        Assert.Contains("ServiceInterceptor.After", InterceptorLog.Entries);
    }

    [Fact]
    public void ServiceInterceptor_WithTypeInterceptor_BothExecute()
    {
        using var host = new TestHost();
        host.Services.AddSingleton<LoggingServiceInterceptor>();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IOrderService>(config =>
        {
            // Service interceptor (resolved from DI container).
            config.Interceptors.AddServiced<LoggingServiceInterceptor>(
                Predicates.Implement(typeof(IOrderService)));
        });

        // PlaceOrder has [OrderLoggingInterceptor] attribute — both interceptors run.
        var result = service.PlaceOrder("widget");
        Assert.Equal("Order placed: widget", result);
        Assert.Contains("ServiceInterceptor.Before", InterceptorLog.Entries);
        Assert.Contains("OrderLoggingInterceptor.Before", InterceptorLog.Entries);
    }

    [Fact]
    public void ServiceInterceptor_MultipleServices_SelectiveInterception()
    {
        using var host = new TestHost();
        host.Services.AddSingleton<LoggingServiceInterceptor>();
        host.Add<ICalculatorService, CalculatorService>();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var provider = host.CreateServiceProvider(config =>
        {
            // Only intercept ICalculatorService.
            config.Interceptors.AddServiced<LoggingServiceInterceptor>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // ICalculatorService is intercepted.
        var calc = provider.GetService<ICalculatorService>();
        Assert.NotNull(calc);
        Assert.Equal(3, calc!.Add(1, 2));
        Assert.Contains("ServiceInterceptor.Before", InterceptorLog.Entries);

        // IOrderService is NOT intercepted (no service interceptor configured for it).
        InterceptorLog.Clear();
        var order = provider.GetService<IOrderService>();
        Assert.NotNull(order);
        Assert.Equal("Order cancelled: x", order!.CancelOrder("x"));
        Assert.DoesNotContain(InterceptorLog.Entries,
            e => e.StartsWith("ServiceInterceptor"));
    }

    [Fact]
    public async Task ServiceInterceptor_AsyncMethod_Works()
    {
        using var host = new TestHost();
        host.Services.AddSingleton<LoggingServiceInterceptor>();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddServiced<LoggingServiceInterceptor>(
                Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();
        Assert.Equal("async-name", result);
        Assert.Contains("ServiceInterceptor.Before", InterceptorLog.Entries);
    }

    /// <summary>
    /// Service interceptor resolved from the DI container. It logs before/after
    /// to InterceptorLog so tests can verify invocation.
    /// </summary>
    public sealed class LoggingServiceInterceptor : IInterceptor
    {
        public bool AllowMultiple => true;

        public bool Inherited { get; set; }

        public int Order { get; set; }

        public async Task Invoke(AspectContext context, AspectDelegate next)
        {
            InterceptorLog.Entries.Add("ServiceInterceptor.Before");
            await context.Invoke(next);
            InterceptorLog.Entries.Add("ServiceInterceptor.After");
        }
    }
}
