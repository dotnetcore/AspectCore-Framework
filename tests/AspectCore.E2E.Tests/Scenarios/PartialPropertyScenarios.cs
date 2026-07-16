using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for C# 13.0 partial properties in proxy generation.
/// Exercises the full pipeline: DI registration → proxy generation (source
/// generator + runtime dynamic proxy) → property accessor invocation.
/// Real DI container, real proxies — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class PartialPropertyScenarios
{
    #region Class Proxy - Partial Property with Implementation

    [Fact]
    public void ClassProxy_PartialProperty_GetSet_DelegatesToBase()
    {
        using var host = new TestHost();
        host.Add<PartialPropertyClassService>();

        var service = host.Resolve<PartialPropertyClassService>();

        // The proxy must delegate property access to the base class implementation.
        service.Name = "e2e-test";
        Assert.Equal("e2e-test", service.Name);
    }

    [Fact]
    public void ClassProxy_PartialProperty_WithInterceptor_Intercepts()
    {
        InterceptorLog.Clear();
        using var host = new TestHost();
        host.Add<PartialPropertyClassService>();

        var service = host.Resolve<PartialPropertyClassService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("PartialProp.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("PartialProp.After");
            }, Predicates.ForService(nameof(PartialPropertyClassService)));
        });

        service.Name = "intercepted";
        Assert.Equal("intercepted", service.Name);
        Assert.Contains("PartialProp.Before", InterceptorLog.Entries);
        Assert.Contains("PartialProp.After", InterceptorLog.Entries);
    }

    #endregion

    #region Class Proxy - Read-Only Partial Property

    [Fact]
    public void ClassProxy_ReadOnlyPartialProperty_DelegatesToBase()
    {
        using var host = new TestHost();
        host.Add<ReadOnlyPartialPropertyClassService>();

        var service = host.Resolve<ReadOnlyPartialPropertyClassService>();

        // The proxy must delegate the read-only property access to the base.
        Assert.Equal("ro-default", service.Name);
    }

    #endregion

    #region Interface Proxy - Partial Property with Target

    [Fact]
    public void InterfaceProxy_PartialProperty_WithTarget_DelegatesToImplementation()
    {
        using var host = new TestHost();
        host.Add<IPartialPropertyInterfaceService, PartialPropertyInterfaceImpl>();

        var service = host.Resolve<IPartialPropertyInterfaceService>();

        // The proxy must delegate property access to the target implementation.
        service.Name = "iface-e2e";
        Assert.Equal("iface-e2e", service.Name);
    }

    [Fact]
    public void InterfaceProxy_PartialProperty_WithTarget_Intercepts()
    {
        InterceptorLog.Clear();
        using var host = new TestHost();
        host.Add<IPartialPropertyInterfaceService, PartialPropertyInterfaceImpl>();

        var service = host.Resolve<IPartialPropertyInterfaceService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("IfaceProp.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("IfaceProp.After");
            }, Predicates.Implement(typeof(IPartialPropertyInterfaceService)));
        });

        service.Name = "iface-intercepted";
        Assert.Equal("iface-intercepted", service.Name);
        Assert.Contains("IfaceProp.Before", InterceptorLog.Entries);
        Assert.Contains("IfaceProp.After", InterceptorLog.Entries);
    }

    #endregion

    #region Interface Proxy - Partial Property Mixed Accessors (get has impl, no set)

    [Fact]
    public void InterfaceProxy_PartialProperty_MixedAccessors_WithTarget_Works()
    {
        using var host = new TestHost();
        host.Add<IPartialPropertyMixedService, PartialPropertyMixedImpl>();

        var service = host.Resolve<IPartialPropertyMixedService>();

        // The get accessor has an implementation; the set accessor is declaration-only.
        Assert.Equal("mixed-default", service.Name);
    }

    #endregion
}

#region Test Types

// Class proxy: partial property with both declaration and implementation.
[AspectCoreGenerateProxy]
public partial class PartialPropertyClassService
{
    private string _name = "";

    public virtual partial string Name { get; set; }

    public virtual partial string Name
    {
        get => _name;
        set => _name = value ?? "";
    }
}

// Class proxy: read-only partial property with implementation.
[AspectCoreGenerateProxy]
public partial class ReadOnlyPartialPropertyClassService
{
    private string _name = "ro-default";

    public virtual partial string Name { get; }

    public virtual partial string Name => _name;
}

// Interface proxy: partial property with implementation provided by target.
[AspectCoreGenerateProxy(typeof(PartialPropertyInterfaceImpl))]
public partial interface IPartialPropertyInterfaceService
{
    partial string Name { get; set; }

    partial string Name
    {
        get => "";
        set { }
    }
}

public class PartialPropertyInterfaceImpl : IPartialPropertyInterfaceService
{
    private string _name = "";

    public string Name
    {
        get => _name;
        set => _name = value ?? "";
    }
}

// Interface proxy: partial property with mixed accessors (get has impl, no set).
[AspectCoreGenerateProxy(typeof(PartialPropertyMixedImpl))]
public partial interface IPartialPropertyMixedService
{
    partial string Name { get; }

    partial string Name => "mixed-default";
}

public class PartialPropertyMixedImpl : IPartialPropertyMixedService
{
    public string Name => "mixed-default";
}

#endregion
