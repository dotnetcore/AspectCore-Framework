using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests that exercise SourceGeneratedAspectContext code paths through
/// the SourceGenerator proxy engine. These tests cover:
/// - Sync methods (basic, void, multi-param)
/// - Async Task&lt;T&gt; methods
/// - Async ValueTask&lt;T&gt; methods (triggers AwaitIfAsyncNativeAotSafe ValueTask&lt;T&gt; path)
/// - Non-generic Task/ValueTask methods
/// - IAsyncEnumerable&lt;T&gt; methods
/// - Methods with ref/out parameters
/// - Class proxy methods (triggers base-call trampoline)
/// - Init-only property setters on class proxies
/// - Generic methods (triggers the generic method fallback path in Complete())
/// - Multiple interceptors
/// - Break() via short-circuit interceptor
/// </summary>
[Collection("InterceptorLog")]
public class NativeAotSourceGeneratedScenarios
{
    // ========================================================================
    // Interface proxy — sync methods
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_SyncAdd_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        Assert.NotNull(service);
        Assert.True(service.IsProxy());
        Assert.Equal(7, service.Add(3, 4));
    }

    [Fact]
    public void SgEngine_InterfaceProxy_SyncVoid_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Should not throw.
        service.DoNothing();
    }

    [Fact]
    public void SgEngine_InterfaceProxy_SyncMultiParam_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        Assert.Equal(6, service.MultiParam(1, 2, 3, "test"));
    }

    [Fact]
    public void SgEngine_InterfaceProxy_SyncConcat_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        Assert.Equal("HelloWorld", service.Concat("Hello", "World"));
    }

    // ========================================================================
    // Interface proxy — async Task<T> methods
    // ========================================================================

    [Fact]
    public async Task SgEngine_InterfaceProxy_AsyncTaskT_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        var result = await service.MultiplyAsync(3, 4);
        Assert.Equal(12, result);
    }

    [Fact]
    public async Task SgEngine_InterfaceProxy_AsyncTask_Void_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Should not throw.
        await service.DoNothingAsync();
    }

    // ========================================================================
    // Interface proxy — async ValueTask<T> methods
    // ========================================================================

    [Fact]
    public async Task SgEngine_InterfaceProxy_AsyncValueTaskT_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        var result = await service.DivideAsync(10, 2);
        Assert.Equal(5, result);
    }

    [Fact]
    public async Task SgEngine_InterfaceProxy_AsyncValueTask_Void_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Should not throw.
        await service.DoNothingValueTaskAsync();
    }

    // ========================================================================
    // Interface proxy — IAsyncEnumerable<T>
    // ========================================================================

    [Fact]
    public async Task SgEngine_InterfaceProxy_AsyncEnumerable_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        var values = new List<int>();
        await foreach (var v in service.GetNumbersAsync(3))
        {
            values.Add(v);
        }
        Assert.Equal(new[] { 1, 2, 3 }, values);
    }

    // ========================================================================
    // Interface proxy — ref/out parameters
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_OutParameter_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        service.GetOutput(5, out int doubled);
        Assert.Equal(10, doubled);
    }

    [Fact]
    public void SgEngine_InterfaceProxy_RefParameter_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        int value = 5;
        service.Increment(ref value);
        Assert.Equal(6, value);
    }

    // ========================================================================
    // Interface proxy — generic methods (triggers generic fallback in Complete())
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_GenericMethod_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        Assert.Equal(42, service.Echo(42));
        Assert.Equal("hello", service.Echo("hello"));
    }

    [Fact]
    public async Task SgEngine_InterfaceProxy_GenericAsyncMethod_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        var result = await service.EchoAsync("async-echo");
        Assert.Equal("async-echo", result);
    }

    // ========================================================================
    // Class proxy — base-call trampoline through SourceGeneratedAspectContext
    // ========================================================================

    [Fact]
    public void SgEngine_ClassProxy_SyncMethod_Works()
    {
        using var host = new TestHost();
        host.Add<SgClassService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<SgClassService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(SgClassService)));
        });

        Assert.NotNull(service);
        Assert.True(service.IsProxy());
        Assert.Equal(9, service.Calculate(3));
    }

    [Fact]
    public void SgEngine_ClassProxy_StringMethod_Works()
    {
        using var host = new TestHost();
        host.Add<SgClassService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<SgClassService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(SgClassService)));
        });

        Assert.Equal("Hello, World", service.Greet("World"));
    }

    [Fact]
    public async Task SgEngine_ClassProxy_AsyncTaskT_Works()
    {
        using var host = new TestHost();
        host.Add<SgClassService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<SgClassService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(SgClassService)));
        });

        var result = await service.ComputeAsync(5);
        Assert.Equal(10, result);
    }

    [Fact]
    public async Task SgEngine_ClassProxy_AsyncValueTaskT_Works()
    {
        using var host = new TestHost();
        host.Add<SgClassService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<SgClassService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(SgClassService)));
        });

        var result = await service.GetLabelAsync();
        Assert.Equal("class-label", result);
    }

    [Fact]
    public void SgEngine_ClassProxy_GenericMethod_Works()
    {
        using var host = new TestHost();
        host.Add<SgClassService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<SgClassService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(SgClassService)));
        });

        Assert.Equal(99, service.Identity(99));
        Assert.Equal("test", service.Identity("test"));
    }

    // ========================================================================
    // Class proxy — init-only properties (triggers MethodReflector fallback)
    // ========================================================================

    [Fact]
    public void SgEngine_ClassProxy_InitOnlyProperty_Works()
    {
        using var host = new TestHost();
        host.Add<SgInitOnlyService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<SgInitOnlyService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(SgInitOnlyService)));
        });

        Assert.NotNull(service);
        Assert.True(service.IsProxy());
        // Default values from the base class.
        Assert.Equal("default:0", service.Describe());
    }

    // ========================================================================
    // Multiple interceptors — verify pipeline ordering with SG engine
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_MultipleInterceptors_OrderedExecution()
    {
        InterceptorLog.Clear();
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddTyped<SgLogInterceptorAttribute>(
                Predicates.Implement(typeof(ISgBasicService)));
            config.Interceptors.AddTyped<SgSecondInterceptorAttribute>(
                Predicates.Implement(typeof(ISgBasicService)));
        });

        var result = service.Add(1, 2);
        Assert.Equal(3, result);

        // Verify pipeline ordering: SgLog (Order=0 by default) runs before SgSecond (Order=2).
        Assert.Contains("SgLog.Before:Add", InterceptorLog.Entries);
        Assert.Contains("SgSecond.Before:Add", InterceptorLog.Entries);
        Assert.Contains("SgSecond.After:Add", InterceptorLog.Entries);
        Assert.Contains("SgLog.After:Add", InterceptorLog.Entries);

        var logBeforeIdx = InterceptorLog.Entries.IndexOf("SgLog.Before:Add");
        var secondBeforeIdx = InterceptorLog.Entries.IndexOf("SgSecond.Before:Add");
        var secondAfterIdx = InterceptorLog.Entries.IndexOf("SgSecond.After:Add");
        var logAfterIdx = InterceptorLog.Entries.IndexOf("SgLog.After:Add");

        Assert.True(logBeforeIdx < secondBeforeIdx);
        Assert.True(secondBeforeIdx < secondAfterIdx);
        Assert.True(secondAfterIdx < logAfterIdx);
    }

    [Fact]
    public async Task SgEngine_InterfaceProxy_MultipleInterceptors_AsyncMethod()
    {
        InterceptorLog.Clear();
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddTyped<SgLogInterceptorAttribute>(
                Predicates.Implement(typeof(ISgBasicService)));
            config.Interceptors.AddTyped<SgSecondInterceptorAttribute>(
                Predicates.Implement(typeof(ISgBasicService)));
        });

        var result = await service.MultiplyAsync(2, 3);
        Assert.Equal(6, result);

        Assert.Contains("SgLog.Before:MultiplyAsync", InterceptorLog.Entries);
        Assert.Contains("SgSecond.Before:MultiplyAsync", InterceptorLog.Entries);
        Assert.Contains("SgSecond.After:MultiplyAsync", InterceptorLog.Entries);
        Assert.Contains("SgLog.After:MultiplyAsync", InterceptorLog.Entries);
    }

    // ========================================================================
    // Return value modification with SG engine
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_ReturnValueModification_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddTyped<SgReturnModifierInterceptorAttribute>(
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // SgReturnModifierInterceptor adds 100 to int results.
        Assert.Equal(107, service.Add(3, 4));
    }

    [Fact]
    public void SgEngine_InterfaceProxy_StringReturnModification_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddTyped<SgReturnModifierInterceptorAttribute>(
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // SgReturnModifierInterceptor appends "_modified" to string results.
        Assert.Equal("AB_modified", service.Concat("A", "B"));
    }

    // ========================================================================
    // Break() — short-circuit interceptor with SG engine
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_Break_SetsDefaultReturnValue()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            // Short-circuit: don't call next, just break.
            config.Interceptors.AddDelegate((ctx, next) => ctx.Break(),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Break should set the return value to default(int) = 0.
        Assert.Equal(0, service.Add(3, 4));
    }

    [Fact]
    public void SgEngine_InterfaceProxy_Break_StringReturnsNull()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => ctx.Break(),
                Predicates.Implement(typeof(ISgBasicService)));
        });

        // Break should set the return value to default(string) = null.
        Assert.Null(service.Concat("A", "B"));
    }

    [Fact]
    public void SgEngine_InterfaceProxy_Break_WithPresetReturnValue_Preserves()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                ctx.ReturnValue = 42;
                return ctx.Break();
            }, Predicates.Implement(typeof(ISgBasicService)));
        });

        // Break with pre-set ReturnValue should preserve it.
        Assert.Equal(42, service.Add(3, 4));
    }

    // ========================================================================
    // AdditionalData / Dispose through SG engine
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_AdditionalData_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var disposed = false;
        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                ctx.AdditionalData["key"] = new DisposableTracker(() => disposed = true);
                return next(ctx);
            }, Predicates.Implement(typeof(ISgBasicService)));
        });

        var result = service.Add(1, 2);
        Assert.Equal(3, result);
        // Context dispose should clean up AdditionalData IDisposables.
        Assert.True(disposed);
    }

    // ========================================================================
    // Interceptor with ServiceProvider access through SG engine
    // ========================================================================

    [Fact]
    public void SgEngine_InterfaceProxy_ServiceProviderAccess_Works()
    {
        using var host = new TestHost();
        host.Add<ISgBasicService, SgBasicService>();
        host.Services.AddSingleton<IMessageProvider, MessageProvider>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<ISgBasicService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                var provider = ctx.ServiceProvider.GetService(typeof(IMessageProvider)) as IMessageProvider;
                Assert.NotNull(provider);
                Assert.Equal("hello-from-provider", provider!.GetMessage());
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(ISgBasicService)));
        });

        var result = service.Add(1, 2);
        Assert.Equal(3, result);
    }

    // ========================================================================
    // Class proxy — multiple interceptors
    // ========================================================================

    [Fact]
    public void SgEngine_ClassProxy_MultipleInterceptors_Works()
    {
        InterceptorLog.Clear();
        using var host = new TestHost();
        host.Add<SgClassService>();
        host.Services.ConfigureDynamicProxyEngine(options =>
        {
            options.Engine = ProxyEngine.SourceGenerator;
        });

        var service = host.Resolve<SgClassService>(config =>
        {
            config.Interceptors.AddTyped<SgLogInterceptorAttribute>(
                Predicates.ForService(nameof(SgClassService)));
            config.Interceptors.AddTyped<SgSecondInterceptorAttribute>(
                Predicates.ForService(nameof(SgClassService)));
        });

        var result = service.Calculate(4);
        Assert.Equal(12, result);

        Assert.Contains("SgLog.Before:Calculate", InterceptorLog.Entries);
        Assert.Contains("SgSecond.Before:Calculate", InterceptorLog.Entries);
        Assert.Contains("SgSecond.After:Calculate", InterceptorLog.Entries);
        Assert.Contains("SgLog.After:Calculate", InterceptorLog.Entries);
    }

    // ========================================================================
    // Helper types
    // ========================================================================

    private sealed class DisposableTracker : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;

        public DisposableTracker(Action onDispose) => _onDispose = onDispose;

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onDispose();
        }
    }

    [Fact]
    public void IAspectContextFactory_DIM_Default_Falls_Back_To_SingleArg()
    {
        // Exercises the DIM default implementation:
        //   AspectContext CreateContext(ctx, delegate) => CreateContext(ctx);
        // by using a custom factory that only implements the original single-arg method.
        var customFactory = new MinimalFactory();
        var ctx = new AspectActivatorContext(
            typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!,
            typeof(SgBasicService).GetMethod(nameof(SgBasicService.Add))!,
            typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!,
            typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!,
            new SgBasicService(), new SgBasicService(), new object[] { 1, 2 });

        // Call the DIM 2-arg overload on the interface reference
        IAspectContextFactory factory = customFactory;
        var context = factory.CreateContext(ctx, new DummyDelegate());
        // DIM falls back to CreateContext(ctx) which MinimalFactory implements
        Assert.NotNull(context);
    }

    [Fact]
    public void AspectCoreGenericHintAttribute_Can_Be_Instantiated()
    {
        // Cover the [AspectCoreGenericHint] attribute definition
        var attr = new AspectCoreGenericHintAttribute(typeof(int), typeof(string));
        Assert.Equal(2, attr.TypeArguments.Length);
        Assert.Equal(typeof(int), attr.TypeArguments[0]);
        Assert.Equal(typeof(string), attr.TypeArguments[1]);
    }

    [Fact]
    public async Task SourceGeneratedAspectContext_DirectConstructor_Complete_UsesInvokeDelegate()
    {
        var serviceMethod = typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!;
        var implementationMethod = typeof(SgBasicService).GetMethod(nameof(SgBasicService.Add))!;
        var proxyMethod = typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!;
        var serviceProvider = new ServiceCollection().BuildServiceProvider();
        var implementation = new SgBasicService();
        var proxy = new SgBasicService();
        var parameters = new object[] { 3, 4 };
        var context = CreateSourceGeneratedContext(
            serviceProvider,
            serviceMethod,
            implementationMethod,
            proxyMethod,
            serviceMethod,
            implementation,
            proxy,
            parameters,
            new ConstantInvokeDelegate(7));

        Assert.Same(serviceProvider, context.ServiceProvider);
        Assert.Same(serviceMethod, context.ServiceMethod);
        Assert.Same(implementationMethod, context.ImplementationMethod);
        Assert.Same(proxyMethod, context.ProxyMethod);
        Assert.Same(serviceMethod, context.PredicateMethod);
        Assert.Same(implementation, context.Implementation);
        Assert.Same(proxy, context.Proxy);
        Assert.Same(parameters, context.Parameters);

        await context.Complete();

        Assert.Equal(7, context.ReturnValue);
        (context as IDisposable)?.Dispose();
    }

    [Fact]
    public void SourceGeneratedAspectContext_ServiceProvider_Throws_WhenMissing()
    {
        var method = typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!;
        var context = CreateSourceGeneratedContext(
            serviceProvider: null,
            serviceMethod: method,
            implementationMethod: method,
            proxyMethod: method,
            predicateMethod: method,
            targetInstance: new SgBasicService(),
            proxyInstance: new SgBasicService(),
            parameters: Array.Empty<object>(),
            invokeDelegate: new ConstantInvokeDelegate(0));

        Assert.Throws<NotSupportedException>(() => context.ServiceProvider);
        (context as IDisposable)?.Dispose();
    }

    [Fact]
    public void SourceGeneratedAspectContext_Dispose_CleansAdditionalData()
    {
        var method = typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!;
        var disposed = false;
        var context = CreateSourceGeneratedContext(
            new ServiceCollection().BuildServiceProvider(),
            method,
            method,
            method,
            method,
            new SgBasicService(),
            new SgBasicService(),
            Array.Empty<object>(),
            new ConstantInvokeDelegate(0));

        context.AdditionalData["disposable"] = new DisposableTracker(() => disposed = true);
        context.AdditionalData["plain"] = "value";

        (context as IDisposable)?.Dispose();
        (context as IDisposable)?.Dispose();

        Assert.True(disposed);
    }

    [Fact]
    public async Task SourceGeneratedAspectContext_Complete_WithNullImplementation_Breaks()
    {
        var method = typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!;
        var context = CreateSourceGeneratedContext(
            new ServiceCollection().BuildServiceProvider(),
            method,
            method,
            method,
            method,
            targetInstance: null,
            proxyInstance: new SgBasicService(),
            parameters: Array.Empty<object>(),
            invokeDelegate: new ConstantInvokeDelegate(42));

        await context.Complete();

        Assert.Equal(0, context.ReturnValue);
        (context as IDisposable)?.Dispose();
    }

    [Fact]
    public async Task SourceGeneratedAspectContext_Break_ByRefReturn_UsesElementDefault()
    {
        var method = typeof(RefReturnService).GetMethod(nameof(RefReturnService.GetValue))!;
        var service = new RefReturnService();
        var context = CreateSourceGeneratedContext(
            new ServiceCollection().BuildServiceProvider(),
            method,
            method,
            method,
            method,
            service,
            service,
            Array.Empty<object>(),
            new ConstantInvokeDelegate(42));

        await context.Break();

        Assert.Equal(0, context.ReturnValue);
        (context as IDisposable)?.Dispose();
    }

    [Fact]
    public async Task SourceGeneratedAspectContext_Invoke_CallsNext()
    {
        var method = typeof(ISgBasicService).GetMethod(nameof(ISgBasicService.Add))!;
        var context = CreateSourceGeneratedContext(
            new ServiceCollection().BuildServiceProvider(),
            method,
            method,
            method,
            method,
            new SgBasicService(),
            new SgBasicService(),
            Array.Empty<object>(),
            new ConstantInvokeDelegate(0));
        var invoked = false;

        await context.Invoke(_ =>
        {
            invoked = true;
            return Task.CompletedTask;
        });

        Assert.True(invoked);
        (context as IDisposable)?.Dispose();
    }

    private static AspectContext CreateSourceGeneratedContext(
        IServiceProvider? serviceProvider,
        MethodInfo serviceMethod,
        MethodInfo implementationMethod,
        MethodInfo proxyMethod,
        MethodInfo predicateMethod,
        object? targetInstance,
        object? proxyInstance,
        object[] parameters,
        IAspectInvokeDelegate invokeDelegate)
    {
        var contextType = typeof(AspectContextFactory).Assembly.GetType(
            "AspectCore.DynamicProxy.SourceGeneratedAspectContext",
            throwOnError: true)!;
        var constructor = contextType.GetConstructor(
            BindingFlags.Instance | BindingFlags.Public,
            binder: null,
            new[]
            {
                typeof(IServiceProvider),
                typeof(MethodInfo),
                typeof(MethodInfo),
                typeof(MethodInfo),
                typeof(MethodInfo),
                typeof(object),
                typeof(object),
                typeof(object[]),
                typeof(IAspectInvokeDelegate)
            },
            modifiers: null);

        Assert.NotNull(constructor);

        return (AspectContext)constructor!.Invoke(new object?[]
        {
            serviceProvider,
            serviceMethod,
            implementationMethod,
            proxyMethod,
            predicateMethod,
            targetInstance,
            proxyInstance,
            parameters,
            invokeDelegate
        });
    }

    private sealed class MinimalFactory : IAspectContextFactory
    {
        public AspectContext CreateContext(AspectActivatorContext activatorContext)
        {
            // Return a minimal context (RuntimeAspectContext via the real factory)
            var realFactory = new AspectContextFactory(new ServiceCollection().BuildServiceProvider());
            return realFactory.CreateContext(activatorContext);
        }

        public void ReleaseContext(AspectContext aspectContext)
        {
            (aspectContext as IDisposable)?.Dispose();
        }
    }

    private sealed class DummyDelegate : IAspectInvokeDelegate
    {
        public object Invoke(object instance, object[] parameters) => null!;
    }

    private sealed class ConstantInvokeDelegate : IAspectInvokeDelegate
    {
        private readonly object? _value;

        public ConstantInvokeDelegate(object? value)
        {
            _value = value;
        }

        public object Invoke(object instance, object[] parameters) => _value!;
    }

    private sealed class RefReturnService
    {
        private int _value;

        public ref int GetValue() => ref _value;
    }
}
