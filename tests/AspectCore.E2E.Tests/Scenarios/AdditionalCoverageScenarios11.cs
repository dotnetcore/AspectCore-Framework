using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Eleventh batch of E2E coverage tests — exercises ProxyGeneratorExtensions
/// (CreateClassProxy/CreateInterfaceProxy overloads),
/// AttributeAdditionalInterceptorSelector (class-level and method-level
/// interceptor attributes, inherited interceptors), AspectCoreGenerateProxyAttribute
/// (constructors, null checks), and AspectContext.Runtime code paths
/// (Complete with null implementation, Break, Dispose with IDisposable data).
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios11
{
    // ========================================================================
    // ProxyGeneratorExtensions — CreateClassProxy overloads
    // ========================================================================

    [Fact]
    public void ProxyGeneratorExtensions_CreateClassProxy_Generic_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var provider = host.CreateServiceProvider();
        var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();

        var proxy = proxyGenerator.CreateClassProxy<CovariantService>();
        Assert.NotNull(proxy);
        Assert.IsNotType<CovariantService>(proxy);
        Assert.Equal("derived", proxy.Get().Name);
    }

    [Fact]
    public void ProxyGeneratorExtensions_CreateClassProxy_WithServiceAndImpl_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var provider = host.CreateServiceProvider();
        var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();

        // Class proxy requires a class type, not an interface.
        var proxy = proxyGenerator.CreateClassProxy<CovariantService, CovariantService>();
        Assert.NotNull(proxy);
        Assert.IsNotType<CovariantService>(proxy);
        Assert.Equal("derived", proxy.Get().Name);
    }

    [Fact]
    public void ProxyGeneratorExtensions_CreateInterfaceProxy_Generic_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var provider = host.CreateServiceProvider();
        var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();

        // Interface proxy without a target — returns default values.
        var proxy = proxyGenerator.CreateInterfaceProxy<ICalculatorService>();
        Assert.NotNull(proxy);
        Assert.IsNotType<CalculatorService>(proxy);
        // No target implementation → returns default(int) = 0.
        Assert.Equal(0, proxy.Add(3, 4));
    }

    [Fact]
    public void ProxyGeneratorExtensions_CreateInterfaceProxy_WithInstance_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var provider = host.CreateServiceProvider();
        var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();

        var instance = new CalculatorService();
        var proxy = proxyGenerator.CreateInterfaceProxy<ICalculatorService>(instance);
        Assert.NotNull(proxy);
        Assert.IsNotType<CalculatorService>(proxy);
        Assert.Equal(7, proxy.Add(3, 4));
    }

    [Fact]
    public void ProxyGeneratorExtensions_CreateInterfaceProxy_WithType_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var provider = host.CreateServiceProvider();
        var proxyGenerator = provider.GetRequiredService<IProxyGenerator>();

        var proxy = proxyGenerator.CreateInterfaceProxy<ICalculatorService, CalculatorService>();
        Assert.NotNull(proxy);
        Assert.IsNotType<CalculatorService>(proxy);
        Assert.Equal(7, proxy.Add(3, 4));
    }

    [Fact]
    public void ProxyGeneratorExtensions_NullGenerator_Throws()
    {
        IProxyGenerator? nullGenerator = null;
        Assert.Throws<ArgumentNullException>(() => nullGenerator!.CreateClassProxy<CovariantService>());
        Assert.Throws<ArgumentNullException>(() => nullGenerator!.CreateClassProxy<ICalculatorService, CalculatorService>());
        Assert.Throws<ArgumentNullException>(() => nullGenerator!.CreateInterfaceProxy<ICalculatorService>());
        Assert.Throws<ArgumentNullException>(() => nullGenerator!.CreateInterfaceProxy<ICalculatorService>(new CalculatorService()));
    }

    // ========================================================================
    // AttributeAdditionalInterceptorSelector — class-level and method-level
    // interceptor attributes, inherited interceptors
    // ========================================================================

    [Fact]
    public void AttributeAdditionalInterceptor_ClassLevelInterceptor_Executes()
    {
        using var host = new TestHost();
        host.Add<IClassInterceptedService, ClassInterceptedServiceImpl>();

        InterceptorLog.Clear();
        var service = host.Resolve<IClassInterceptedService>();

        var result = service.DoWork();

        Assert.Equal("class-intercepted", result);
        Assert.Contains("ClassLevel.Before", InterceptorLog.Entries);
        Assert.Contains("ClassLevel.After", InterceptorLog.Entries);
    }

    [Fact]
    public void AttributeAdditionalInterceptor_MethodLevelInterceptor_Executes()
    {
        using var host = new TestHost();
        host.Add<IMethodInterceptedService, MethodInterceptedServiceImpl>();

        InterceptorLog.Clear();
        var service = host.Resolve<IMethodInterceptedService>();

        var result = service.Intercepted();

        Assert.Equal("method-intercepted", result);
        Assert.Contains("MethodLevel.Before", InterceptorLog.Entries);
        Assert.Contains("MethodLevel.After", InterceptorLog.Entries);
    }

    [Fact]
    public void AttributeAdditionalInterceptor_MethodLevel_NonInterceptedMethod_Skips()
    {
        using var host = new TestHost();
        host.Add<IMethodInterceptedService, MethodInterceptedServiceImpl>();

        InterceptorLog.Clear();
        var service = host.Resolve<IMethodInterceptedService>();

        var result = service.NotIntercepted();

        Assert.Equal("not-intercepted", result);
        Assert.DoesNotContain(InterceptorLog.Entries, e => e.StartsWith("MethodLevel"));
    }

    [Fact]
    public void AttributeAdditionalInterceptor_InheritedFromBaseClass_Executes()
    {
        using var host = new TestHost();
        host.Add<IInheritedInterceptedService, InheritedInterceptedServiceImpl>();

        InterceptorLog.Clear();
        var service = host.Resolve<IInheritedInterceptedService>();

        var result = service.DoWork();

        Assert.Equal("inherited-intercepted", result);
        // The base class has an interceptor attribute that should be inherited.
        Assert.Contains("InheritedLevel.Before", InterceptorLog.Entries);
        Assert.Contains("InheritedLevel.After", InterceptorLog.Entries);
    }

    // ========================================================================
    // AspectCoreGenerateProxyAttribute — constructors and null checks
    // ========================================================================

    [Fact]
    public void AspectCoreGenerateProxyAttribute_DefaultConstructor_SetsDefaults()
    {
        var attr = new AspectCoreGenerateProxyAttribute();
        Assert.Null(attr.ServiceType);
        Assert.Null(attr.ImplementationType);
        Assert.Null(attr.Kind);
    }

    [Fact]
    public void AspectCoreGenerateProxyAttribute_WithServiceAndImpl_SetsProperties()
    {
        var attr = new AspectCoreGenerateProxyAttribute(
            typeof(ICalculatorService), typeof(CalculatorService), SourceGeneratedProxyKind.Interface);
        Assert.Equal(typeof(ICalculatorService), attr.ServiceType);
        Assert.Equal(typeof(CalculatorService), attr.ImplementationType);
        Assert.Equal(SourceGeneratedProxyKind.Interface, attr.Kind);
    }

    [Fact]
    public void AspectCoreGenerateProxyAttribute_WithImplType_SetsImplementationType()
    {
        var attr = new AspectCoreGenerateProxyAttribute(typeof(CalculatorService));
        Assert.Null(attr.ServiceType);
        Assert.Equal(typeof(CalculatorService), attr.ImplementationType);
        Assert.Null(attr.Kind);
    }

    [Fact]
    public void AspectCoreGenerateProxyAttribute_NullServiceType_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AspectCoreGenerateProxyAttribute(null!, typeof(CalculatorService), SourceGeneratedProxyKind.Interface));
    }

    [Fact]
    public void AspectCoreGenerateProxyAttribute_NullImplementationType_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AspectCoreGenerateProxyAttribute(typeof(ICalculatorService), null!, SourceGeneratedProxyKind.Interface));
    }

    [Fact]
    public void AspectCoreGenerateProxyAttribute_NullImplTypeInSingleArgCtor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AspectCoreGenerateProxyAttribute(null!));
    }

    // ========================================================================
    // AspectContext.Runtime — Complete, Break, Dispose code paths
    // ========================================================================

    [Fact]
    public void AspectContext_Break_SetsDefaultReturnValue()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            // Short-circuit: don't call next, just break.
            config.Interceptors.AddDelegate((ctx, next) => ctx.Break(),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Break should set the return value to default(int) = 0.
        var result = service.Add(3, 4);
        Assert.Equal(0, result);
    }

    [Fact]
    public void AspectContext_Break_WithNullReturnValue_SetsDefaultForReferenceType()
    {
        using var host = new TestHost();
        host.Add<IOrderService, OrderService>();

        var service = host.Resolve<IOrderService>(config =>
        {
            // Short-circuit with break — return value should be null for reference types.
            config.Interceptors.AddDelegate((ctx, next) => ctx.Break(),
                Predicates.Implement(typeof(IOrderService)));
        });

        var result = service.PlaceOrder("test");
        Assert.Null(result);
    }

    [Fact]
    public void AspectContext_AdditionalData_Disposable_IsDisposed()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var disposed = false;
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                // Add IDisposable data to AdditionalData.
                ctx.AdditionalData["test"] = new DisposableData(() => disposed = true);
                return next(ctx);
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(1, 2);
        Assert.Equal(3, result);

        // The context should be disposed after invocation, disposing the AdditionalData.
        Assert.True(disposed);
    }

    // ========================================================================
    // Helper types
    // ========================================================================

    public interface IClassInterceptedService
    {
        string DoWork();
    }

    [ClassLevelInterceptor]
    public class ClassInterceptedServiceImpl : IClassInterceptedService
    {
        public string DoWork() => "class-intercepted";
    }

    public interface IMethodInterceptedService
    {
        string Intercepted();
        string NotIntercepted();
    }

    public class MethodInterceptedServiceImpl : IMethodInterceptedService
    {
        [MethodLevelInterceptor]
        public string Intercepted() => "method-intercepted";

        public string NotIntercepted() => "not-intercepted";
    }

    public interface IInheritedInterceptedService
    {
        string DoWork();
    }

    [InheritedLevelInterceptor]
    public class InheritedInterceptedServiceBase
    {
        public virtual string DoWork() => "base";
    }

    public class InheritedInterceptedServiceImpl : InheritedInterceptedServiceBase, IInheritedInterceptedService
    {
        public override string DoWork() => "inherited-intercepted";
    }

    public sealed class ClassLevelInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            InterceptorLog.Entries.Add("ClassLevel.Before");
            await context.Invoke(next);
            InterceptorLog.Entries.Add("ClassLevel.After");
        }
    }

    public sealed class MethodLevelInterceptorAttribute : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            InterceptorLog.Entries.Add("MethodLevel.Before");
            await context.Invoke(next);
            InterceptorLog.Entries.Add("MethodLevel.After");
        }
    }

    public sealed class InheritedLevelInterceptorAttribute : AbstractInterceptorAttribute
    {
        public InheritedLevelInterceptorAttribute()
        {
            Inherited = true;
        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            InterceptorLog.Entries.Add("InheritedLevel.Before");
            await context.Invoke(next);
            InterceptorLog.Entries.Add("InheritedLevel.After");
        }
    }

    public sealed class DisposableData : IDisposable
    {
        private readonly Action _onDispose;
        private bool _disposed;

        public DisposableData(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _onDispose();
        }
    }
}
