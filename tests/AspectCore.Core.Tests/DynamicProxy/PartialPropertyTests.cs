#nullable enable

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy;

/// <summary>
/// Tests for C# 13.0 partial properties support in both the source generator
/// and runtime dynamic proxy engines.
/// </summary>
public class PartialPropertyTests
{
    #region Class Proxy - Partial Property with Implementation

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_PartialProperty_WithImplementation_ShouldDelegateToBase(ProxyEngine engine)
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

        var proxy = proxyGenerator.CreateClassProxy<PartialPropertyClassWithImpl>();

        // The base class has a partial property with both declaration and implementation.
        // The proxy should delegate to the base implementation.
        proxy.Name = "test";
        Assert.Equal("test", proxy.Name);
        Assert.Equal(2, snapshots.Count);
        Assert.Equal(new[] { "set_Name", "get_Name" }, snapshots.Select(snapshot => snapshot.ServiceMethodName));
    }

    #endregion

    #region Class Proxy - Partial Property Getter Only with Implementation

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_PartialProperty_ReadOnly_WithImplementation_ShouldDelegateToBase(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<PartialPropertyReadOnlyClassWithImpl>();

        // The base class has a read-only partial property with implementation.
        Assert.Equal("default-value", proxy.Name);
    }

    #endregion

    #region Interface Proxy - Partial Property with Implementation in Target

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void InterfaceProxy_PartialProperty_WithTarget_ShouldDelegateToImplementation(ProxyEngine engine)
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

        var implementation = new PartialPropertyImpl();
        var proxy = proxyGenerator.CreateInterfaceProxy<IPartialPropertyService>(implementation);

        // The interface has a partial property, and the implementation provides the body.
        proxy.Name = "hello";
        Assert.Equal("hello", proxy.Name);
    }

    #endregion

    #region Interface Proxy - Partial Property with Mixed Accessors (get has impl, set is declaration-only)

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void InterfaceProxy_PartialProperty_MixedAccessors_ShouldWork(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        // The interface has a partial property where the get accessor has an
        // implementation (returns "stub-default") but there is no set accessor.
        // Use a target implementation that provides the property value.
        var implementation = new PartialPropertyMixedImpl();
        var proxy = proxyGenerator.CreateInterfaceProxy<IPartialPropertyMixedService>(implementation);

        // Getter should return the implementation's value.
        Assert.Equal("stub-default", proxy.Name);
    }

    #endregion

    #region Concrete Class - Partial Property with Implementation

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ConcreteClass_PartialProperty_WithImplementation_DelegatesToBase(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        // A non-abstract class has a partial property with both declaration and
        // implementation parts. The proxy must delegate to the actual implementation.
        var proxy = proxyGenerator.CreateClassProxy<ConcretePartialPropertyWithImpl>();

        proxy.Name = "concrete-impl";
        Assert.Equal("concrete-impl", proxy.Name);
    }

    #endregion


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

#region Test Type Definitions

// Class proxy: partial property with both declaration and implementation
[AspectCoreGenerateProxy]
public partial class PartialPropertyClassWithImpl
{
    private string _name = "";

    public virtual partial string Name { get; set; }

    public virtual partial string Name
    {
        get => _name;
        set => _name = value ?? "";
    }
}

// Class proxy: read-only partial property with implementation
[AspectCoreGenerateProxy]
public partial class PartialPropertyReadOnlyClassWithImpl
{
    private string _name = "default-value";

    public virtual partial string Name { get; }

    public virtual partial string Name => _name;
}

// Interface proxy: partial property with implementation provided as default interface member
[AspectCoreGenerateProxy(typeof(PartialPropertyImpl))]
public partial interface IPartialPropertyService
{
    partial string Name { get; set; }

    partial string Name
    {
        get => "";
        set { }
    }
}

public class PartialPropertyImpl : IPartialPropertyService
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set => _name = value ?? "";
    }
}

// Interface proxy: partial property with declaration + implementation in interface
// (with target implementation). The get accessor has an implementation but there
// is no set accessor.
[AspectCoreGenerateProxy(typeof(PartialPropertyMixedImpl))]
public partial interface IPartialPropertyMixedService
{
    partial string? Name { get; }

    partial string? Name => "stub-default";
}

public class PartialPropertyMixedImpl : IPartialPropertyMixedService
{
    public string? Name => "stub-default";
}

// Non-abstract, non-sealed class: partial property with both declaration and
// implementation parts. The proxy delegates to the compiled implementation.
[AspectCoreGenerateProxy]
public partial class ConcretePartialPropertyWithImpl
{
    private string _name = "";

    public partial string Name { get; set; }

    public partial string Name
    {
        get => _name;
        set => _name = value ?? "";
    }
}

#endregion
