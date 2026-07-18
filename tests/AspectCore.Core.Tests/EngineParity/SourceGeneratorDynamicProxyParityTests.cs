#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

public class SourceGeneratorDynamicProxyParityTests
{
    [Fact]
    public void SourceGenerator_Strict_Missing_Generated_Proxy_Should_Fail_With_Clear_Message()
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            ProxyEngine.SourceGenerator,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: true,
            allowRuntimeFallback: false);

        Assert.IsType<SourceGeneratedProxyTypeGenerator>(proxyGenerator.TypeGenerator);

        var ex = Assert.Throws<InvalidOperationException>(() =>
            proxyGenerator.CreateClassProxy<MissingProxyService>());

        Assert.Contains("Failed to resolve source-generated proxy type.", ex.Message, StringComparison.Ordinal);
        Assert.Contains("Engine: SourceGenerator", ex.Message, StringComparison.Ordinal);
        Assert.Contains(typeof(MissingProxyService).FullName!, ex.Message, StringComparison.Ordinal);
        Assert.Contains("Hint:", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void AutoEngine_Should_RuntimeFallback_To_DynamicProxy_When_Missing_Generated_Proxy()
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            ProxyEngine.Auto,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: false,
            allowRuntimeFallback: null);

        var proxy = proxyGenerator.CreateClassProxy<MissingProxyService>();
        Assert.True(proxy.IsProxy());
        Assert.Equal(1, proxy.Get());
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public async Task ReturnKind_Should_Work_And_Be_Consistent(ProxyEngine engine)
    {
        var snapshots = new ConcurrentQueue<AspectSnapshot>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    snapshots.Enqueue(AspectSnapshot.Capture(ctx));
                    await ctx.Invoke(next);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<ReturnKindsService>();

        Assert.Equal(11, proxy.Sync(10));

        proxy.Void(42);
        Assert.Equal(42, proxy.LastVoidArg);

        await proxy.TaskVoid();
        Assert.Equal(1, proxy.TaskVoidCalls);

        Assert.Equal(21, await proxy.TaskOfT(20));

        await proxy.ValueTaskVoid();
        Assert.Equal(1, proxy.ValueTaskVoidCalls);

        var stream = new List<int>();
        await foreach (var value in proxy.AsyncEnumerable())
        {
            stream.Add(value);
        }
        Assert.Equal(new[] { 1, 2 }, stream);

        Assert.Equal(31, await proxy.ValueTaskOfT(30));

        // basic context sanity for all invoked methods
        var invoked = snapshots.Select(s => s.ServiceMethodName).ToHashSet(StringComparer.Ordinal);
        Assert.Contains(nameof(ReturnKindsService.Sync), invoked);
        Assert.Contains(nameof(ReturnKindsService.Void), invoked);
        Assert.Contains(nameof(ReturnKindsService.TaskVoid), invoked);
        Assert.Contains(nameof(ReturnKindsService.TaskOfT), invoked);
        Assert.Contains(nameof(ReturnKindsService.ValueTaskVoid), invoked);
        Assert.Contains(nameof(ReturnKindsService.AsyncEnumerable), invoked);
        Assert.Contains(nameof(ReturnKindsService.ValueTaskOfT), invoked);

        foreach (var s in snapshots)
        {
            Assert.NotNull(s.ServiceMethod);
            Assert.NotNull(s.ImplementationMethod);
            Assert.NotNull(s.ProxyMethod);

            Assert.False(string.IsNullOrWhiteSpace(s.ServiceMethodDisplay));
            Assert.False(string.IsNullOrWhiteSpace(s.ProxyMethodDisplay));
        }
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void RefOut_Should_Roundtrip_Value_Reference_And_Nullable(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);

                    // mutate parameters after invocation to verify proxy write-back
                    switch (ctx.ServiceMethod.Name)
                    {
                        case nameof(RefOutService.RefValue):
                            ctx.Parameters[0] = 123;
                            break;
                        case nameof(RefOutService.RefNullableStruct):
                            ctx.Parameters[0] = (int?)456;
                            break;
                        case nameof(RefOutService.RefReference):
                            ctx.Parameters[0] = "lemon";
                            break;
                        case nameof(RefOutService.RefNullableReference):
                            ctx.Parameters[0] = (string?)null;
                            break;
                        case nameof(RefOutService.OutValue):
                            ctx.Parameters[0] = 789;
                            break;
                        case nameof(RefOutService.OutNullableReference):
                            ctx.Parameters[0] = (string?)"nullable";
                            break;
                    }
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<RefOutService>();

        var i = 0;
        proxy.RefValue(ref i);
        Assert.Equal(123, i);

        int? ni = null;
        proxy.RefNullableStruct(ref ni);
        Assert.Equal(456, ni);

        var s = "x";
        proxy.RefReference(ref s);
        Assert.Equal("lemon", s);

        string? ns = "not-null";
        proxy.RefNullableReference(ref ns);
        Assert.Null(ns);

        proxy.OutValue(out var o);
        Assert.Equal(789, o);

        proxy.OutNullableReference(out var os);
        Assert.Equal("nullable", os);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public async Task Exception_Propagation_And_ThrowAspectException_Should_Match(ProxyEngine engine)
    {
        await AssertThrow(engine, throwAspectException: false);
        await AssertThrow(engine, throwAspectException: true);
    }

    private static async Task AssertThrow(ProxyEngine engine, bool throwAspectException)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.ThrowAspectException = throwAspectException;
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<ThrowingService>();

        AssertThrown(() => proxy.ThrowSync(), throwAspectException);
        await AssertThrownAsync(() => proxy.ThrowTask(), throwAspectException);
        await AssertThrownAsync(async () => await proxy.ThrowValueTask(), throwAspectException);
    }

    private static void AssertThrown(Action action, bool throwAspectException)
    {
        if (!throwAspectException)
        {
            var ex = Assert.Throws<InvalidOperationException>(action);
            Assert.Equal(ThrowingService.Message, ex.Message);
            return;
        }

        var wrapped = Assert.Throws<AspectInvocationException>(action);
        Assert.IsType<InvalidOperationException>(wrapped.InnerException);
        Assert.Equal(ThrowingService.Message, wrapped.InnerException!.Message);
        Assert.NotNull(wrapped.AspectContext);
    }

    private static async Task AssertThrownAsync(Func<Task> action, bool throwAspectException)
    {
        if (!throwAspectException)
        {
            var ex = await Assert.ThrowsAsync<InvalidOperationException>(action);
            Assert.Equal(ThrowingService.Message, ex.Message);
            return;
        }

        var wrapped = await Assert.ThrowsAsync<AspectInvocationException>(action);
        Assert.IsType<InvalidOperationException>(wrapped.InnerException);
        Assert.Equal(ThrowingService.Message, wrapped.InnerException!.Message);
        Assert.NotNull(wrapped.AspectContext);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void AspectContext_Metadata_Should_Be_Aligned_Invariants(ProxyEngine engine)
    {
        var snapshots = new ConcurrentQueue<AspectSnapshot>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    snapshots.Enqueue(AspectSnapshot.Capture(ctx));
                    await ctx.Invoke(next);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        // class proxy
        var classProxy = proxyGenerator.CreateClassProxy<MetadataClassService>();
        Assert.Equal("v:payload", classProxy.Combine<string>("v", "payload"));

        // interface proxy with target
        var ifaceProxy = proxyGenerator.CreateInterfaceProxy<IMetadataService>(new MetadataInterfaceService());
        Assert.Equal("v:payload", ifaceProxy.Combine<string>("v", "payload"));

        var all = snapshots.ToArray();
        Assert.True(all.Length >= 2);

        foreach (var s in all)
        {
            Assert.NotNull(s.ServiceMethod);
            Assert.NotNull(s.ImplementationMethod);
            Assert.NotNull(s.ProxyMethod);

            // ServiceMethod / ImplementationMethod should always be real user methods
            Assert.NotNull(s.ServiceMethod!.DeclaringType);
            Assert.NotNull(s.ImplementationMethod!.DeclaringType);
            Assert.NotNull(s.ProxyMethod!.DeclaringType);

            // proxy method must come from proxy type
            Assert.True(s.ProxyMethod.DeclaringType!.IsDefined(typeof(DynamicallyAttribute), inherit: false));

            // generic method: should align on generic definition
            if (s.ServiceMethod.IsGenericMethod)
            {
                var sm = s.ServiceMethod;
                var im = s.ImplementationMethod;

                Assert.True(sm.IsGenericMethod);
                Assert.True(im.IsGenericMethod);
                Assert.Equal(sm.Name, im.Name);
                Assert.Equal(sm.GetGenericArguments().Length, im.GetGenericArguments().Length);
                Assert.Equal(sm.GetParameters().Length, im.GetParameters().Length);

                // For class proxies (service method & implementation method come from same declaring type),
                // the generic definition should be exactly the same MethodInfo.
                if (sm.DeclaringType == im.DeclaringType)
                {
                    Assert.Equal(sm.GetGenericMethodDefinition(), im.GetGenericMethodDefinition());
                }
            }
        }

        var classCall = all.Single(x => x.ServiceDeclaringType == typeof(MetadataClassService));
        Assert.Equal(typeof(MetadataClassService), classCall.ServiceMethod!.DeclaringType);
        Assert.Equal(typeof(MetadataClassService), classCall.ImplementationMethod!.DeclaringType);

        var ifaceCall = all.Single(x => x.ServiceDeclaringType == typeof(IMetadataService));
        Assert.Equal(typeof(IMetadataService), ifaceCall.ServiceMethod!.DeclaringType);
        Assert.Equal(typeof(MetadataInterfaceService), ifaceCall.ImplementationMethod!.DeclaringType);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void Explicit_Interface_Implementation_And_DIM_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                // Only intercept GetVal to match historical behavior
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next), Predicates.ForMethod("GetVal"));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        // explicit interface impl via class proxy
        var explicitProxy = proxyGenerator.CreateClassProxy<ExplicitImplementationService>();
        var iface = (IExplicitImplementationService)explicitProxy;
        Assert.Equal("lemon", iface.GetVal());
        Assert.Equal("lemon", iface.GetVal_NonAspect());
        Assert.Equal(1, iface.GetVal2());

        // DIM via interface proxy (no target)
        var dimProxy = proxyGenerator.CreateInterfaceProxy<IDefaultMethodService>();
        Assert.Equal(1, dimProxy.Get());
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void NonAspect_And_NonAspectPredicates_Should_Not_Intercept(ProxyEngine engine)
    {
        var called = new ConcurrentQueue<string>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    called.Enqueue(ctx.ServiceMethod.Name);
                    await ctx.Invoke(next);
                    ctx.ReturnValue = "intercepted";
                });

                cfg.NonAspectPredicates.Add(m => m.Name == nameof(NonAspectPredicateService.NotInterceptedByPredicate));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        // [NonAspect] on method
        var nonAspectProxy = proxyGenerator.CreateInterfaceProxy<INonAspectService>(new NonAspectService());
        Assert.Equal("intercepted", nonAspectProxy.Intercepted());
        Assert.Equal("raw", nonAspectProxy.NotIntercepted());

        // NonAspectPredicates
        var predProxy = proxyGenerator.CreateClassProxy<NonAspectPredicateService>();
        Assert.Equal("intercepted", predProxy.InterceptedByPredicate());
        Assert.Equal("raw", predProxy.NotInterceptedByPredicate());

        // Verify interceptor did not run for NonAspect / predicate excluded methods
        var callSet = called.ToArray().ToHashSet(StringComparer.Ordinal);
        Assert.Contains(nameof(INonAspectService.Intercepted), callSet);
        Assert.DoesNotContain(nameof(INonAspectService.NotIntercepted), callSet);
        Assert.Contains(nameof(NonAspectPredicateService.InterceptedByPredicate), callSet);
        Assert.DoesNotContain(nameof(NonAspectPredicateService.NotInterceptedByPredicate), callSet);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void Interceptor_PropertyInjection_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddTyped<PropertyInjectionInterceptor>(Predicates.ForService("*PropertyInjectionService"));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null,
            configureService: serviceContext =>
            {
                serviceContext.AddDelegate<PropertyInjectionDependency>(_ => new PropertyInjectionDependency { Value = "injected" });
            });

        var proxy = proxyGenerator.CreateClassProxy<PropertyInjectionService>();
        Assert.Equal("injected", proxy.GetValue());
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ServiceInstance_PropertyInjection_Should_Work(ProxyEngine engine)
    {
        // Verify that when a service with [FromServiceContext] properties is resolved
        // through the DI container (which triggers PropertyInjectorCallback), the
        // properties are injected correctly. This tests the full DI resolution path.
        var builder = new ServiceContext();
        builder.Configuration.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next),
            Predicates.ForService("*ServiceWithPropertyInjection"));
        builder.AddDelegate<PropertyInjectionDependency>(_ => new PropertyInjectionDependency { Value = "svc-injected" });
        builder.AddType<ServiceWithPropertyInjection>();

        // Set engine options for SG
        var engineOptions = new ProxyEngineOptions
        {
            Engine = engine,
            Strict = engine == ProxyEngine.SourceGenerator,
            AllowRuntimeFallback = engine == ProxyEngine.SourceGenerator ? false : (bool?)null,
        };
        builder.AddInstance(engineOptions);
        if (engine != ProxyEngine.DynamicProxy)
        {
            builder.RemoveAll(typeof(IProxyTypeGenerator));
            builder.AddInstance<IProxyTypeGenerator>(
                new SourceGeneratedProxyTypeGenerator(
                    new AspectValidatorBuilder(builder.Configuration),
                    engineOptions,
                    Array.Empty<ISourceGeneratedProxyRegistry>()));
        }

        var resolver = builder.Build();
        var service = resolver.Resolve<ServiceWithPropertyInjection>();
        Assert.NotNull(service);
        Assert.Equal("svc-injected", service.Echo());
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ParameterAspect_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.EnableParameterAspect();
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next),
                    Predicates.ForService("*ParameterService"));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<ParameterService>();
        // The NotNull parameter interceptor should throw for null input
        Assert.Throws<ArgumentNullException>(() => proxy.Greet(null!));
        Assert.Equal("Hello, World", proxy.Greet("World"));
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void GenericClassProxy_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    // Double the return value to prove interception works
                    if (ctx.ReturnValue is int i)
                        ctx.ReturnValue = i * 2;
                }, Predicates.ForService("*GenericService*"));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        // Create generic proxy with int type argument
        var proxy = proxyGenerator.CreateClassProxy(typeof(GenericService<int>), typeof(GenericService<int>), Array.Empty<object>());
        Assert.NotNull(proxy);

        // Verify the proxy is actually a proxy (intercepts calls)
        var echoMethod = proxy.GetType().GetMethod("Echo");
        Assert.NotNull(echoMethod);

        // Call Echo(21) - interceptor doubles the return value
        var result = echoMethod.Invoke(proxy, new object[] { 21 });
        Assert.Equal(42, result); // 21 * 2 from interceptor
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void GenericClassProxy_WithConstraint_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next),
                    Predicates.ForService("*GenericServiceWithConstraint*"));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy(
            typeof(GenericServiceWithConstraint<string>),
            typeof(GenericServiceWithConstraint<string>),
            Array.Empty<object>());
        Assert.NotNull(proxy);

        var method = proxy.GetType().GetMethod("FirstOrDefault");
        Assert.NotNull(method);

        // Test with non-empty array
        var result = method.Invoke(proxy, new object[] { new[] { "hello", "world" } });
        Assert.Equal("hello", result);

        // Test with empty array
        result = method.Invoke(proxy, new object[] { Array.Empty<string>() });
        Assert.Null(result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void RecordClassProxy_WithExpression_Should_Copy_Proxy_And_Keep_Interception(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await ctx.Invoke(next);
                    if (ctx.ServiceMethod.Name == nameof(RecordClassService.Label))
                    {
                        ctx.ReturnValue = $"intercepted:{ctx.ReturnValue}";
                    }
                }, Predicates.ForService("*RecordClassService"));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<RecordClassService>("alpha", 1);
        Assert.True(proxy.IsProxy());
        Assert.Equal("intercepted:alpha:1", proxy.Label());

        var copy = proxy with { Name = "beta" };

        Assert.True(copy.IsProxy());
        Assert.Equal(proxy.GetType(), copy.GetType());
        Assert.Equal("beta", copy.Name);
        Assert.Equal(1, copy.Count);
        Assert.Equal("intercepted:beta:1", copy.Label());
    }

    private sealed record AspectSnapshot(
        Type ServiceDeclaringType,
        MethodInfo? ServiceMethod,
        MethodInfo? ImplementationMethod,
        MethodInfo? ProxyMethod,
        string ServiceMethodName,
        string ServiceMethodDisplay,
        string ProxyMethodDisplay)
    {
        public static AspectSnapshot Capture(AspectContext ctx)
        {
            var sm = ctx.ServiceMethod;
            var im = ctx.ImplementationMethod;
            var pm = ctx.ProxyMethod;
            return new AspectSnapshot(
                ServiceDeclaringType: sm?.DeclaringType ?? typeof(object),
                ServiceMethod: sm,
                ImplementationMethod: im,
                ProxyMethod: pm,
                ServiceMethodName: sm?.Name ?? string.Empty,
                ServiceMethodDisplay: sm?.ToString() ?? string.Empty,
                ProxyMethodDisplay: pm?.ToString() ?? string.Empty);
        }
    }
}

[AspectCoreGenerateProxy]
public class ReturnKindsService
{
    public int LastVoidArg { get; private set; }
    public int TaskVoidCalls { get; private set; }
    public int ValueTaskVoidCalls { get; private set; }

    public virtual int Sync(int value) => value + 1;

    public virtual void Void(int value) => LastVoidArg = value;

    public virtual Task TaskVoid()
    {
        TaskVoidCalls++;
        return Task.CompletedTask;
    }

    public virtual Task<int> TaskOfT(int value) => Task.FromResult(value + 1);

    public virtual ValueTask ValueTaskVoid()
    {
        ValueTaskVoidCalls++;
        return ValueTask.CompletedTask;
    }

    public virtual async IAsyncEnumerable<int> AsyncEnumerable()
    {
        await Task.Yield();
        yield return 1;
        yield return 2;
    }

    public virtual ValueTask<int> ValueTaskOfT(int value) => ValueTask.FromResult(value + 1);
}

[AspectCoreGenerateProxy]
public class RefOutService
{
    public virtual void RefValue(ref int value) { }

    public virtual void RefNullableStruct(ref int? value) { }

    public virtual void RefReference(ref string value) { }

    public virtual void RefNullableReference(ref string? value) { }

    public virtual void OutValue(out int value) => value = 0;

    public virtual void OutNullableReference(out string? value) => value = null;
}

[AspectCoreGenerateProxy]
public class ThrowingService
{
    public const string Message = "boom";

    public virtual void ThrowSync() => throw new InvalidOperationException(Message);

    public virtual Task ThrowTask() => Task.FromException(new InvalidOperationException(Message));

    public virtual ValueTask ThrowValueTask() => ValueTask.FromException(new InvalidOperationException(Message));
}

[AspectCoreGenerateProxy]
public interface IMetadataService
{
    string Combine<TPayload>(string prefix, TPayload payload) where TPayload : class;
}

public class MetadataInterfaceService : IMetadataService
{
    public string Combine<TPayload>(string prefix, TPayload payload) where TPayload : class
        => $"{prefix}:{payload}";
}

[AspectCoreGenerateProxy]
public class MetadataClassService
{
    public virtual string Combine<TPayload>(string prefix, TPayload payload) where TPayload : class
        => $"{prefix}:{payload}";
}

public interface IExplicitImplementationService
{
    string GetVal();

    string GetVal_NonAspect();

    int GetVal2();
}

[AspectCoreGenerateProxy]
public class ExplicitImplementationService : IExplicitImplementationService
{
    string IExplicitImplementationService.GetVal() => "lemon";

    int IExplicitImplementationService.GetVal2() => 1;

    string IExplicitImplementationService.GetVal_NonAspect() => "lemon";
}

[AspectCoreGenerateProxy]
public interface IDefaultMethodService
{
    int Get() => 1;
}

[AspectCoreGenerateProxy]
public interface INonAspectService
{
    string Intercepted();

    [NonAspect]
    string NotIntercepted();
}

public class NonAspectService : INonAspectService
{
    public string Intercepted() => "raw";

    public string NotIntercepted() => "raw";
}

[AspectCoreGenerateProxy]
public class NonAspectPredicateService
{
    public virtual string InterceptedByPredicate() => "raw";

    public virtual string NotInterceptedByPredicate() => "raw";
}

public class MissingProxyService
{
    public virtual int Get() => 1;
}

// ── Property Injection test types ──────────────────────────────────────

public class PropertyInjectionDependency
{
    public string Value { get; set; } = "";
}

public class PropertyInjectionInterceptor : AbstractInterceptor
{
    [FromServiceContext]
    public PropertyInjectionDependency Dependency { get; set; } = null!;

    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        context.ReturnValue = Dependency?.Value ?? "null";
        return Task.CompletedTask;
    }
}

[AspectCoreGenerateProxy]
public class PropertyInjectionService
{
    public virtual string GetValue() => "original";
}

[AspectCoreGenerateProxy]
public class ServiceWithPropertyInjection
{
    [FromServiceContext]
    public PropertyInjectionDependency Dependency { get; set; } = null!;

    public virtual string Echo() => Dependency?.Value ?? "null";
}

// ── Parameter Aspect test types ────────────────────────────────────────

[AspectCoreGenerateProxy]
public class ParameterService
{
    public virtual string Greet([NotNull] string name) => $"Hello, {name}";
}

// ── Generic Class Proxy test types ─────────────────────────────────────

[AspectCoreGenerateProxy]
public class GenericService<T>
{
    public virtual T Echo(T value) => value;

    public virtual string Describe(T value) => typeof(T).Name + ": " + value;
}

[AspectCoreGenerateProxy]
public class GenericServiceWithConstraint<T> where T : class
{
    public virtual T? FirstOrDefault(T[] items) => items.Length > 0 ? items[0] : null;
}

[AspectCoreGenerateProxy]
public record RecordClassService(string Name, int Count)
{
    public virtual string Label() => $"{Name}:{Count}";
}
