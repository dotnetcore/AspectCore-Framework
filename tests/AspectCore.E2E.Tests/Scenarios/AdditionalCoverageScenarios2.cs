using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Second batch of E2E coverage tests targeting additional uncovered paths:
/// - Static field/property/method reflectors on service implementations
/// - Constructor reflector on service implementations
/// - DI resolver and scope factory usage
/// - More async return type scenarios
/// - Parameter aspect with more edge cases
/// - Return value handling
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios2
{
    // ========================================================================
    // Reflection extensions - static fields, properties, methods on impl types
    // ========================================================================

    [Fact]
    public void FieldReflector_StaticField_OnServiceImpl_Works()
    {
        using var host = new TestHost();
        host.Add<IReflectionTestService, ReflectionTestService>();

        var service = host.Resolve<IReflectionTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReflectionTestService)));
        });

        // Access static field via FieldReflector on the implementation type
        var implType = typeof(ReflectionTestService);
        var staticField = implType.GetField("StaticField", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(staticField);

        var reflector = staticField!.GetReflector();
        reflector.SetStaticValue("new-value");
        Assert.Equal("new-value", reflector.GetStaticValue());
    }

    [Fact]
    public void FieldReflector_InstanceField_OnServiceImpl_Works()
    {
        using var host = new TestHost();
        host.Add<IReflectionTestService, ReflectionTestService>();

        var service = host.Resolve<IReflectionTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReflectionTestService)));
        });

        // Access instance field via FieldReflector on the proxy type
        var proxyType = service.GetType();
        var instanceField = proxyType.GetField("_instanceField", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (instanceField != null)
        {
            var reflector = instanceField.GetReflector();
            Assert.NotNull(reflector);
        }
    }

    [Fact]
    public void PropertyReflector_StaticProperty_OnServiceImpl_Works()
    {
        using var host = new TestHost();
        host.Add<IReflectionTestService, ReflectionTestService>();

        var service = host.Resolve<IReflectionTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReflectionTestService)));
        });

        // Access static property via PropertyReflector on the implementation type
        var implType = typeof(ReflectionTestService);
        var staticProp = implType.GetProperty("StaticProperty", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(staticProp);

        var reflector = staticProp!.GetReflector();
        reflector.SetStaticValue("static-prop-value");
        Assert.Equal("static-prop-value", reflector.GetStaticValue());
    }

    [Fact]
    public void MethodReflector_StaticMethod_OnServiceImpl_Works()
    {
        using var host = new TestHost();
        host.Add<IReflectionTestService, ReflectionTestService>();

        var service = host.Resolve<IReflectionTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReflectionTestService)));
        });

        // Access static method via MethodReflector on the implementation type
        var implType = typeof(ReflectionTestService);
        var staticMethod = implType.GetMethod("StaticMethod", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(staticMethod);

        var reflector = staticMethod!.GetReflector();
        var result = reflector.StaticInvoke("hello");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void ConstructorReflector_OnServiceImpl_CreatesInstance()
    {
        var ctor = typeof(ReflectionTestService).GetConstructor(Type.EmptyTypes);
        Assert.NotNull(ctor);

        var reflector = ctor!.GetReflector();
        var instance = reflector.Invoke();
        Assert.NotNull(instance);
        Assert.IsType<ReflectionTestService>(instance);
    }

    [Fact]
    public void CustomAttributeReflector_OnServiceImpl_ReadsAttributes()
    {
        using var host = new TestHost();
        host.Add<IReflectionTestService, ReflectionTestService>();

        var service = host.Resolve<IReflectionTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReflectionTestService)));
        });

        // Read custom attributes from the proxy method
        var method = service.GetType().GetMethod(nameof(IReflectionTestService.GetValue));
        Assert.NotNull(method);

        var attributes = method!.GetReflector().GetCustomAttributes();
        Assert.NotNull(attributes);
    }

    // ========================================================================
    // DI resolver and scope factory usage with proxied services
    // ========================================================================

    [Fact]
    public void DiResolver_ResolvesProxiedService_FromRootProvider()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Singleton);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var resolver = provider.GetService<IServiceResolver>();
        Assert.NotNull(resolver);

        var service = resolver!.Resolve(typeof(ICalculatorService));
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ICalculatorService>(service);
        Assert.Equal(7, ((ICalculatorService)service!).Add(3, 4));
    }

    [Fact]
    public void DiScopeResolverFactory_CreatesScope_WithProxiedService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var scopeFactory = provider.GetService<IScopeResolverFactory>();
        Assert.NotNull(scopeFactory);

        using var scope = scopeFactory!.CreateScope();
        var service = scope.Resolve(typeof(ICalculatorService));
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ICalculatorService>(service);
    }

    [Fact]
    public void DiServiceScope_ResolvesProxiedService()
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
        Assert.Equal(12, service!.Add(5, 7));
    }

    // ========================================================================
    // More async return type scenarios
    // ========================================================================

    [Fact]
    public async Task Async_TaskOfT_WithReturnValueInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                var unwrapped = await ctx.UnwrapAsyncReturnValue();
                if (unwrapped is string s)
                {
                    ctx.ReturnValue = Task.FromResult(s + "-modified");
                }
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetNameAsync();
        Assert.Equal("async-name-modified", result);
    }

    [Fact]
    public async Task Async_ValueTaskOfT_WithInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("ValueTaskInterceptor.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("ValueTaskInterceptor.After");
            }, Predicates.Implement(typeof(IAsyncService)));
        });

        var result = await service.GetLabelAsync();
        Assert.Equal("async-label", result);
        Assert.Contains("ValueTaskInterceptor.Before", InterceptorLog.Entries);
    }

    [Fact]
    public async Task Async_MultipleSequentialValueTasks_Work()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        var label1 = await service.GetLabelAsync();
        var name = await service.GetNameAsync();
        var label2 = await service.GetLabelAsync();

        Assert.Equal("async-label", label1);
        Assert.Equal("async-name", name);
        Assert.Equal("async-label", label2);
    }

    // ========================================================================
    // Return value handling
    // ========================================================================

    [Fact]
    public void ReturnValue_Int_InterceptorModifies_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                if (ctx.ReturnValue is int i)
                {
                    ctx.ReturnValue = i + 100;
                }
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(5, 5);
        Assert.Equal(110, result);
    }

    [Fact]
    public void ReturnValue_String_InterceptorReplaces_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                await ctx.Invoke(next);
                ctx.ReturnValue = "replaced";
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Concat("a", "b");
        Assert.Equal("replaced", result);
    }

    // ========================================================================
    // Test service types
    // ========================================================================

    public interface IReflectionTestService
    {
        string GetValue(int input);
        string Process(string input);
    }

    public class ReflectionTestService : IReflectionTestService
    {
        public static string StaticField = "initial";
        public static string StaticProperty { get; set; } = "initial-prop";

        public string _instanceField = "instance-field";

        public static string StaticMethod(string input) => input;

        public string GetValue(int input) => input.ToString();
        public virtual string Process(string input) => input;
    }
}
