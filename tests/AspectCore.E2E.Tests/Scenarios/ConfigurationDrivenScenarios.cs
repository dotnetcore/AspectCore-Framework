using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for configuration-driven interceptors: predicates (ForService,
/// ForMethod, ForNameSpace), wildcard matching, multiple interceptors via
/// configuration, mixing attribute + configuration interceptors, and
/// configuration binding injection. Real configuration, real proxies — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ConfigurationDrivenScenarios
{
    [Fact]
    public void Interceptor_ConfiguredVia_ForService_Executes()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("ForService.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("ForService.After");
            }, Predicates.ForService(nameof(ICalculatorService)));
        });

        var result = service.Add(1, 2);

        Assert.Equal(3, result);
        Assert.Contains("ForService.Before", InterceptorLog.Entries);
        Assert.Contains("ForService.After", InterceptorLog.Entries);
    }

    [Fact]
    public void Interceptor_ConfiguredVia_ForMethod_Executes()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Only the Add method should be intercepted.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("ForMethod.Add");
                await ctx.Invoke(next);
            }, Predicates.ForMethod("Add"));
        });

        // Add is intercepted.
        Assert.Equal(5, service.Add(2, 3));
        Assert.Contains("ForMethod.Add", InterceptorLog.Entries);

        // Subtract is NOT intercepted (different method name).
        InterceptorLog.Clear();
        Assert.Equal(1, service.Subtract(5, 4));
        Assert.DoesNotContain(InterceptorLog.Entries, e => e.StartsWith("ForMethod"));
    }

    [Fact]
    public void Interceptor_ConfiguredVia_ForNameSpace_Executes()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Match the exact namespace of ICalculatorService.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("ForNameSpace.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("ForNameSpace.After");
            }, Predicates.ForNameSpace("AspectCore.E2E.Tests.Fixtures"));
        });

        var result = service.Add(10, 20);

        Assert.Equal(30, result);
        Assert.Contains("ForNameSpace.Before", InterceptorLog.Entries);
        Assert.Contains("ForNameSpace.After", InterceptorLog.Entries);
    }

    [Fact]
    public void WildcardMatching_ForService_WithStarPattern_Executes()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Wildcard: matches any type whose name ends with "Service".
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Wildcard.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Wildcard.After");
            }, Predicates.ForService("*Service"));
        });

        var result = service.Add(3, 4);

        Assert.Equal(7, result);
        Assert.Contains("Wildcard.Before", InterceptorLog.Entries);
        Assert.Contains("Wildcard.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task WildcardMatching_ForMethod_WithStarPattern_Executes()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            // Wildcard: matches any method whose name ends with "Async".
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("WildcardMethod.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("WildcardMethod.After");
            }, Predicates.ForMethod("*Async"));
        });

        var result = await service.GetNameAsync();

        Assert.Equal("async-name", result);
        Assert.Contains("WildcardMethod.Before", InterceptorLog.Entries);
        Assert.Contains("WildcardMethod.After", InterceptorLog.Entries);
    }

    [Fact]
    public void MultipleInterceptors_ViaConfiguration_AllExecute()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Config.First");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Config.First.After");
            }, Predicates.Implement(typeof(ICalculatorService)));

            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Config.Second");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Config.Second.After");
            }, Predicates.Implement(typeof(ICalculatorService)));

            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Config.Third");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Config.Third.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(1, 1);

        Assert.Equal(2, result);
        // All three interceptors execute in registration order.
        Assert.Equal("Config.First", InterceptorLog.Entries[0]);
        Assert.Equal("Config.Second", InterceptorLog.Entries[1]);
        Assert.Equal("Config.Third", InterceptorLog.Entries[2]);
        Assert.Equal("Config.Third.After", InterceptorLog.Entries[3]);
        Assert.Equal("Config.Second.After", InterceptorLog.Entries[4]);
        Assert.Equal("Config.First.After", InterceptorLog.Entries[5]);
    }

    [Fact]
    public void Mixed_AttributeAndConfiguration_BothExecute()
    {
        // PlaceOrder has [OrderLoggingInterceptor] attribute; we also add a
        // configuration-based interceptor. Both should execute.
        using var host = new TestHost();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IOrderService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("ConfigInterceptor.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("ConfigInterceptor.After");
            }, Predicates.Implement(typeof(IOrderService)));
        });

        var result = service.PlaceOrder("widget");

        // Both the attribute interceptor and the config interceptor execute.
        Assert.Equal("Order placed: widget", result);
        Assert.Contains("OrderLoggingInterceptor.Before", InterceptorLog.Entries);
        Assert.Contains("ConfigInterceptor.Before", InterceptorLog.Entries);
        Assert.Contains("ConfigInterceptor.After", InterceptorLog.Entries);
        Assert.Contains("OrderLoggingInterceptor.After", InterceptorLog.Entries);
    }

    [Fact]
    public void ConfigurationPredicate_DoesNotMatch_OtherService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();
        host.Add<IOrderService, OrderService>();

        InterceptorLog.Clear();
        var provider = host.CreateServiceProvider(config =>
        {
            // Only intercept ICalculatorService, not IOrderService.
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("CalculatorOnly");
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        // ICalculatorService is intercepted.
        var calc = provider.GetService<ICalculatorService>();
        Assert.NotNull(calc);
        Assert.Equal(3, calc!.Add(1, 2));
        Assert.Contains("CalculatorOnly", InterceptorLog.Entries);

        // IOrderService is NOT intercepted (no proxy generated for it).
        InterceptorLog.Clear();
        var order = provider.GetService<IOrderService>();
        Assert.NotNull(order);
        // CancelOrder has no attribute interceptor, so it should not be logged.
        Assert.Equal("Order cancelled: x", order!.CancelOrder("x"));
        Assert.DoesNotContain(InterceptorLog.Entries, e => e == "CalculatorOnly");
    }

    [Fact]
    public void ConfigurationBinding_Injection_ThroughProxy_Works()
    {
        // Register a configuration object and a service that depends on it.
        // The proxy must correctly inject the configuration into the constructor.
        using var host = new TestHost();
        var config = new ServiceConfiguration { Greeting = "Hello from config" };
        host.Services.AddSingleton(config);
        host.Add<IConfiguredService, ConfiguredService>();

        var service = host.Resolve<IConfiguredService>(cfg =>
        {
            cfg.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IConfiguredService)));
        });

        // The configuration was injected through the proxy pipeline.
        Assert.Equal("Hello from config", service.GetGreeting());
        Assert.IsNotType<ConfiguredService>(service);
    }

    /// <summary>
    /// Simple configuration object for constructor injection testing.
    /// </summary>
    public class ServiceConfiguration
    {
        public string Greeting { get; set; } = string.Empty;
    }

    /// <summary>
    /// Service that receives configuration through constructor injection.
    /// </summary>
    public interface IConfiguredService
    {
        string GetGreeting();
    }

    /// <summary>
    /// Real implementation that depends on ServiceConfiguration via constructor injection.
    /// </summary>
    public class ConfiguredService : IConfiguredService
    {
        private readonly ServiceConfiguration _config;

        public ConfiguredService(ServiceConfiguration config)
        {
            _config = config;
        }

        public string GetGreeting() => _config.Greeting;
    }
}
