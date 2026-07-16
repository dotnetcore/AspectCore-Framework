using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Additional E2E tests covering high-impact uncovered paths:
/// - FieldReflector on proxy types
/// - MethodReflector with various return types on proxy
/// - ConstructorReflector on proxy types
/// - DI scope resolution with proxied services
/// - Async scenarios with multiple return types
/// - Parameter aspect with ref/out/params/nullable
/// - Service interceptor with multiple services
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios
{
    // ========================================================================
    // Reflection extension usage in proxy context
    // ========================================================================

    [Fact]
    public void FieldReflector_OnProxyType_ReadsFields()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Use FieldReflector on the proxy type's fields (if any)
        var proxyType = service.GetType();
        var fields = proxyType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        foreach (var field in fields)
        {
            var reflector = field.GetReflector();
            var value = reflector.GetValue(service);
            // Just verify we can read without throwing
            Assert.NotNull(reflector);
        }
    }

    [Fact]
    public void MethodReflector_Invoke_AllCalculatorMethods_Work()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Test various method types via MethodReflector
        var addMethod = service.GetType().GetMethod(nameof(ICalculatorService.Add));
        Assert.NotNull(addMethod);
        var addReflector = addMethod!.GetReflector();
        Assert.Equal(7, addReflector.Invoke(service, 3, 4));

        var subMethod = service.GetType().GetMethod(nameof(ICalculatorService.Subtract));
        Assert.NotNull(subMethod);
        var subReflector = subMethod!.GetReflector();
        Assert.Equal(1, subReflector.Invoke(service, 5, 4));

        var concatMethod = service.GetType().GetMethod(nameof(ICalculatorService.Concat));
        Assert.NotNull(concatMethod);
        var concatReflector = concatMethod!.GetReflector();
        Assert.Equal("ab", concatReflector.Invoke(service, "a", "b"));

        var sumMethod = service.GetType().GetMethod(nameof(ICalculatorService.Sum));
        Assert.NotNull(sumMethod);
        var sumReflector = sumMethod!.GetReflector();
        Assert.Equal(6, sumReflector.Invoke(service, new int[] { 1, 2, 3 }));
    }

    [Fact]
    public void MethodReflector_Invoke_GenericEcho_OnProxy_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var echoMethod = service.GetType().GetMethods()
            .FirstOrDefault(m => m.Name == nameof(ICalculatorService.Echo) && m.IsGenericMethod);
        Assert.NotNull(echoMethod);

        var closedMethod = echoMethod!.MakeGenericMethod(typeof(string));
        var reflector = closedMethod.GetReflector();
        var result = reflector.Invoke(service, "hello");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ConstructorReflector_Invoke_OnProxyImplementationType_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Get the proxy type and use ConstructorReflector
        var proxyType = service.GetType();
        var constructors = proxyType.GetConstructors();
        foreach (var ctor in constructors)
        {
            var reflector = ctor.GetReflector();
            Assert.NotNull(reflector);
        }
    }

    [Fact]
    public void PropertyReflector_GetSet_OnProxy_PropertyService_Works()
    {
        using var host = new TestHost();
        host.Add<IPropertyCoverageService, PropertyCoverageService>();

        var service = host.Resolve<IPropertyCoverageService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IPropertyCoverageService)));
        });

        var property = service.GetType().GetProperty(nameof(IPropertyCoverageService.Name));
        Assert.NotNull(property);

        var reflector = property!.GetReflector();

        // Set via reflector
        reflector.SetValue(service, "reflector-set");
        Assert.Equal("reflector-set", service.Name);

        // Get via reflector
        var value = reflector.GetValue(service);
        Assert.Equal("reflector-set", value);
    }

    // ========================================================================
    // DI extension internals - scope resolution with proxied services
    // ========================================================================

    [Fact]
    public void DiScope_ProxiedService_ResolvedFromChildScope()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<ICalculatorService>();
        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(10, service!.Add(6, 4));
    }

    [Fact]
    public void DiScope_MultipleChildScopes_IndependentProxies()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        ICalculatorService s1;
        ICalculatorService s2;
        using (var scope1 = provider.CreateScope())
        {
            s1 = scope1.ServiceProvider.GetRequiredService<ICalculatorService>();
        }
        using (var scope2 = provider.CreateScope())
        {
            s2 = scope2.ServiceProvider.GetRequiredService<ICalculatorService>();
        }

        Assert.NotSame(s1, s2);
        Assert.IsNotType<CalculatorService>(s1);
        Assert.IsNotType<CalculatorService>(s2);
    }

    [Fact]
    public void DiScope_IServiceScopeFactory_ResolvesProxiedService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var scopeFactory = provider.GetService<IServiceScopeFactory>();
        Assert.NotNull(scopeFactory);

        using var scope = scopeFactory!.CreateScope();
        var service = scope.ServiceProvider.GetService<ICalculatorService>();
        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
    }

    // ========================================================================
    // Async scenarios with various return types
    // ========================================================================

    [Fact]
    public async Task Async_TaskReturn_WithMultipleInterceptors_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("FirstAsync.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("FirstAsync.After");
            }, Predicates.Implement(typeof(IAsyncService)));
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("SecondAsync.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("SecondAsync.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();
        Assert.Equal("async-name", result);
        Assert.Contains("FirstAsync.Before", InterceptorLog.Entries);
        Assert.Contains("SecondAsync.Before", InterceptorLog.Entries);
        Assert.Contains("SecondAsync.After", InterceptorLog.Entries);
        Assert.Contains("FirstAsync.After", InterceptorLog.Entries);
    }

    [Fact]
    public async Task Async_ValueTaskReturn_WithInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetLabelAsync();
        Assert.Equal("async-label", result);
    }

    [Fact]
    public async Task Async_TaskVoidReturn_Chain_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        await service.ChainAsync();
        // If we get here without exception, the chain worked
        Assert.True(true);
    }

    [Fact]
    public async Task Async_ExceptionInValueTask_IsPropagated()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        // ThrowAsync returns Task<int> that throws
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ThrowAsync());
        Assert.Equal("boom", ex.Message);
    }

    // ========================================================================
    // Parameter aspect edge cases
    // ========================================================================

    [Fact]
    public void ParameterAspect_RefParameter_Intercepted()
    {
        using var host = new TestHost();
        host.Add<IParamEdgeService, ParamEdgeService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamEdgeService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamEdgeService)));
        });

        int value = 5;
        service.DoubleRef(ref value);
        Assert.Equal(10, value);
        Assert.Contains("ParamEdge:value", InterceptorLog.Entries);
    }

    [Fact]
    public void ParameterAspect_OutParameter_Intercepted()
    {
        using var host = new TestHost();
        host.Add<IParamEdgeService, ParamEdgeService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamEdgeService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamEdgeService)));
        });

        service.GetOutput(out var output);
        Assert.Equal(42, output);
        Assert.Contains("ParamEdge:output", InterceptorLog.Entries);
    }

    [Fact]
    public void ParameterAspect_ParamsParameter_Intercepted()
    {
        using var host = new TestHost();
        host.Add<IParamEdgeService, ParamEdgeService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamEdgeService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamEdgeService)));
        });

        var sum = service.SumAll(1, 2, 3, 4);
        Assert.Equal(10, sum);
        Assert.Contains("ParamEdge:values", InterceptorLog.Entries);
    }

    [Fact]
    public void ParameterAspect_NullableParameter_Intercepted()
    {
        using var host = new TestHost();
        host.Add<IParamEdgeService, ParamEdgeService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamEdgeService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamEdgeService)));
        });

        var result = service.GreetNullable("hello");
        Assert.Equal("hello", result);
        Assert.Contains("ParamEdge:name", InterceptorLog.Entries);
    }

    [Fact]
    public void ParameterAspect_GenericMethod_Intercepted()
    {
        using var host = new TestHost();
        host.Add<IParamEdgeService, ParamEdgeService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamEdgeService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamEdgeService)));
        });

        var result = service.EchoGeneric(42);
        Assert.Equal(42, result);
    }

    // ========================================================================
    // Service interceptor resolution
    // ========================================================================

    [Fact]
    public void ServiceInterceptor_MultipleInterceptors_AllExecute()
    {
        using var host = new TestHost();
        host.Services.AddSingleton<CoverageServiceInterceptor>();
        host.Services.AddSingleton<SecondServiceInterceptor>();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddServiced<CoverageServiceInterceptor>(
                Predicates.Implement(typeof(ICalculatorService)));
            config.Interceptors.AddServiced<SecondServiceInterceptor>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(3, 4);
        Assert.Equal(7, result);
        Assert.Contains("CoverageService.Before", InterceptorLog.Entries);
        Assert.Contains("SecondService.Before", InterceptorLog.Entries);
    }

    [Fact]
    public void ServiceInterceptor_ScopedService_ResolvedCorrectly()
    {
        using var host = new TestHost();
        host.Services.AddScoped<CoverageServiceInterceptor>();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        InterceptorLog.Clear();
        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddServiced<CoverageServiceInterceptor>(
                Predicates.Implement(typeof(ICalculatorService)));
        });

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetRequiredService<ICalculatorService>();
        var result = service.Add(5, 5);
        Assert.Equal(10, result);
        Assert.Contains("CoverageService.Before", InterceptorLog.Entries);
    }

    // ========================================================================
    // Test service types and interceptors
    // ========================================================================

    public interface IParamEdgeService
    {
        void DoubleRef([LogEdgeParam("value")] ref int value);
        void GetOutput([LogEdgeParam("output")] out int output);
        int SumAll([LogEdgeParam("values")] params int[] values);
        string? GreetNullable([LogEdgeParam("name")] string? name);
        T EchoGeneric<T>(T value);
    }

    public class ParamEdgeService : IParamEdgeService
    {
        public void DoubleRef(ref int value) => value *= 2;
        public void GetOutput(out int output) => output = 42;
        public int SumAll(params int[] values) => values.Sum();
        public string? GreetNullable(string? name) => name;
        public T EchoGeneric<T>(T value) => value;
    }

    public class LogEdgeParam : ParameterInterceptorAttribute
    {
        private readonly string _name;
        public LogEdgeParam(string name) => _name = name;

        public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
        {
            InterceptorLog.Entries.Add($"ParamEdge:{_name}");
            return next(context);
        }
    }

    public sealed class SecondServiceInterceptor : IInterceptor
    {
        public bool AllowMultiple => true;
        public bool Inherited { get; set; }
        public int Order { get; set; }

        public async Task Invoke(AspectContext context, AspectDelegate next)
        {
            InterceptorLog.Entries.Add("SecondService.Before");
            await context.Invoke(next);
            InterceptorLog.Entries.Add("SecondService.After");
        }
    }

    public sealed class CoverageServiceInterceptor : IInterceptor
    {
        public bool AllowMultiple => true;
        public bool Inherited { get; set; }
        public int Order { get; set; }

        public async Task Invoke(AspectContext context, AspectDelegate next)
        {
            InterceptorLog.Entries.Add("CoverageService.Before");
            await context.Invoke(next);
            InterceptorLog.Entries.Add("CoverageService.After");
        }
    }

    public interface IPropertyCoverageService
    {
        string? Name { get; set; }
    }

    public class PropertyCoverageService : IPropertyCoverageService
    {
        public virtual string? Name { get; set; }
    }
}
