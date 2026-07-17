#if NET7_0_OR_GREATER
#nullable enable

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.EngineParity;

public class InitRequiredMembersParityTests
{
    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ClassProxy_InitOnlyRequiredProperty_Should_PreserveMetadata_And_Intercept(ProxyEngine engine)
    {
        var calledMethods = new System.Collections.Concurrent.ConcurrentQueue<string>();
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

        var proxy = proxyGenerator.CreateClassProxy<InitRequiredClassService>("created");

        Assert.True(proxy.IsProxy());
        Assert.Equal("created", proxy.Name);
        Assert.Equal("created!", proxy.Format());
        Assert.Contains(nameof(InitRequiredClassService.Format), calledMethods);

        var proxyType = proxy.GetType();
        var property = Assert.Single(proxyType.GetProperties(), p => p.Name == nameof(InitRequiredClassService.Name));
        Assert.True(IsRequiredProperty(property), "Proxy property should preserve required metadata.");
        Assert.True(IsInitOnly(property), "Proxy property setter should preserve init-only modreq.");

        var ctor = Assert.Single(proxyType.GetConstructors(), c => c.IsDefined(typeof(SetsRequiredMembersAttribute), inherit: false));
        Assert.True(ctor.IsDefined(typeof(SetsRequiredMembersAttribute), inherit: false));
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void InterfaceProxy_InitOnlyProperty_Should_PreserveSetterMetadata(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateInterfaceProxy<IInitInterfaceService>(new InitInterfaceService("target"));

        Assert.True(proxy.IsProxy());
        Assert.Equal("target", proxy.Name);

        var property = Assert.Single(proxy.GetType().GetProperties(), p => p.Name == nameof(IInitInterfaceService.Name));
        Assert.True(IsInitOnly(property), "Proxy interface property setter should preserve init-only modreq.");
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void InterfaceProxyWithoutTarget_InitOnlyProperty_Should_PreserveSetterMetadata(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateInterfaceProxy<IInitInterfaceService>();

        Assert.True(proxy.IsProxy());
        var property = Assert.Single(proxy.GetType().GetProperties(), p => p.Name == nameof(IInitInterfaceService.Name));
        Assert.True(IsInitOnly(property), "Proxy interface stub property setter should preserve init-only modreq.");
    }

    private static bool IsRequiredProperty(System.Reflection.PropertyInfo property)
    {
        return property.IsDefined(typeof(RequiredMemberAttribute), inherit: false);
    }

    private static bool IsInitOnly(System.Reflection.PropertyInfo property)
    {
        var setMethod = property.SetMethod;
        Assert.NotNull(setMethod);
        return setMethod!.ReturnParameter.GetRequiredCustomModifiers()
            .Any(t => t.FullName == "System.Runtime.CompilerServices.IsExternalInit");
    }
}

[AspectCoreGenerateProxy]
public class InitRequiredClassService
{
    [SetsRequiredMembers]
    public InitRequiredClassService(string name)
    {
        Name = name;
    }

    public required virtual string Name { get; init; }

    public virtual string Format() => Name + "!";
}

[AspectCoreGenerateProxy(typeof(InitInterfaceService))]
public interface IInitInterfaceService
{
    string Name { get; init; }
}

public class InitInterfaceService : IInitInterfaceService
{
    public InitInterfaceService(string name)
    {
        Name = name;
    }

    public string Name { get; init; }
}
#endif
