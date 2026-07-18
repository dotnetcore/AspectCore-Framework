using System.Collections.Concurrent;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

/// <summary>
/// Parity tests for C# 7.0 <c>ref</c> / <c>ref readonly</c> return methods across
/// both AOP engines (DynamicProxy IL emit + Source Generator).
///
/// The interceptor pipeline is value-based (<see cref="AspectContext.ReturnValue"/>
/// is an <see cref="object"/>), so an intercepted <c>ref</c> return is materialised
/// into a StrongBox&lt;T&gt; and returned by ref. Consequently, reads observe the
/// (possibly interceptor-replaced) value, but writes through the returned ref do NOT
/// flow back to the target's original storage — mirroring the ref/out parameter
/// copy-back semantics. See docs/3.CSharp-Language-Features-AOP-Emit-Adaptation.md 6.6.
/// </summary>
public class RefReturnParityTests
{
    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_RefReturn_Should_ReadValue_And_Intercept(ProxyEngine engine)
    {
        var calledMethods = new ConcurrentQueue<string>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) =>
                {
                    calledMethods.Enqueue(ctx.ServiceMethod.Name);
                    return ctx.Invoke(next);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<RefReturnClassService>();

        Assert.True(proxy.IsProxy());
        ref int slot = ref proxy.GetSlot();
        Assert.Equal(7, slot);
        Assert.Contains(nameof(RefReturnClassService.GetSlot), calledMethods);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_RefReturn_Interceptor_Should_ReplaceReturnValue(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await next(ctx);
                    // Replace the materialised return value; the proxy hands back a ref
                    // to this replaced value.
                    ctx.ReturnValue = 99;
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<RefReturnClassService>();

        ref int slot = ref proxy.GetSlot();
        Assert.Equal(99, slot);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_RefReturn_ReturnedRef_Is_Writable(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<RefReturnClassService>();

        // The returned ref aliases a heap slot (StrongBox) and is writable; the write
        // is observable through that same ref, though it does not propagate to the
        // target's backing field under interception (value-based pipeline).
        ref int slot = ref proxy.GetSlot();
        slot = 123;
        Assert.Equal(123, slot);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_RefReadOnlyReturn_Should_ReadValue(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<RefReturnClassService>();

        ref readonly int slot = ref proxy.GetReadOnlySlot();
        Assert.Equal(11, slot);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_RefReturn_ReferenceType_Should_ReadValue(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<RefReturnClassService>();

        ref string slot = ref proxy.GetReference();
        Assert.Equal("hello", slot);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_GenericRefReturn_Should_ReadValue(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<GenericRefReturnClassService>();

        ref int slot = ref proxy.Echo(31);
        Assert.Equal(31, slot);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void InterfaceProxy_WithTarget_RefReturn_Should_ReadValue_And_Intercept(ProxyEngine engine)
    {
        var calledMethods = new ConcurrentQueue<string>();
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) =>
                {
                    calledMethods.Enqueue(ctx.ServiceMethod.Name);
                    return ctx.Invoke(next);
                });
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateInterfaceProxy<IRefReturnService>(new RefReturnService());

        Assert.True(proxy.IsProxy());
        ref int slot = ref proxy.GetSlot();
        Assert.Equal(42, slot);
        Assert.Contains(nameof(IRefReturnService.GetSlot), calledMethods);
    }
}

[AspectCoreGenerateProxy]
public class RefReturnClassService
{
    private int _value = 7;
    private int _readOnlyValue = 11;
    private string _reference = "hello";

    public virtual ref int GetSlot() => ref _value;

    public virtual ref readonly int GetReadOnlySlot() => ref _readOnlyValue;

    public virtual ref string GetReference() => ref _reference;
}

[AspectCoreGenerateProxy]
public class GenericRefReturnClassService
{
    private object _slot;

    // Generic ref return: the target stores the value in a boxed field and hands
    // back a ref to a per-call holder so the returned managed pointer is valid.
    public virtual ref T Echo<T>(T value)
    {
        var holder = new Holder<T> { Value = value };
        _slot = holder;
        return ref holder.Value;
    }

    private sealed class Holder<T>
    {
        public T Value;
    }
}

[AspectCoreGenerateProxy(typeof(RefReturnService))]
public interface IRefReturnService
{
    ref int GetSlot();
}

public class RefReturnService : IRefReturnService
{
    private int _value = 42;

    public ref int GetSlot() => ref _value;
}
