#nullable enable

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.EngineParity;

/// <summary>
/// 边界情况测试：验证 Source Generator 在复杂场景下的正确性
/// </summary>
public class SourceGeneratorEdgeCaseTests
{
    #region Interface Proxy with Target 测试

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void InterfaceProxyWithTarget_Should_Find_Correct_Implementation_Method(ProxyEngine engine)
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

        var implementation = new ServiceImplementation();
        // 使用带实现类型的接口代理
        var proxy = proxyGenerator.CreateInterfaceProxy<IServiceWithTarget_Proxy>(implementation);

        // 调用方法
        var result = proxy.DoWork("test");

        Assert.Equal("test:processed", result);
        Assert.Single(snapshots);

        var snapshot = snapshots.Single();

        // 验证 ServiceMethod 指向接口方法
        Assert.Equal(typeof(IServiceWithTarget_Proxy), snapshot.ServiceMethod?.DeclaringType);
        Assert.Equal(nameof(IServiceWithTarget_Proxy.DoWork), snapshot.ServiceMethod?.Name);

        // 验证 ImplementationMethod 指向实现类的方法
        Assert.Equal(typeof(ServiceImplementation), snapshot.ImplementationMethod?.DeclaringType);
        Assert.Equal(nameof(IServiceWithTarget_Proxy.DoWork), snapshot.ImplementationMethod?.Name);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void InterfaceProxyWithTarget_Generic_Should_Work(ProxyEngine engine)
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

        var implementation = new GenericServiceImplementation();
        // 使用带实现类型的泛型接口代理
        var proxy = proxyGenerator.CreateInterfaceProxy<IGenericServiceWithTarget_Proxy>(implementation);

        // 调用泛型方法
        var result = proxy.Process(42);

        Assert.Equal(84, result);
        Assert.Single(snapshots);

        var snapshot = snapshots.Single();
        Assert.True(snapshot.ServiceMethod?.IsGenericMethod ?? false);
        Assert.True(snapshot.ImplementationMethod?.IsGenericMethod ?? false);
    }

    #endregion

    #region 泛型方法边界测试

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void GenericMethod_Single_Type_Parameter_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<GenericMethodService>();

        // 单个泛型参数
        var result = proxy.SingleTypeParam(42);
        Assert.Equal(42, result);

        var strResult = proxy.SingleTypeParam("hello");
        Assert.Equal("hello", strResult);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void GenericMethod_Multiple_Type_Parameters_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<GenericMethodService>();

        // 多个泛型参数
        var result = proxy.MultipleTypeParams<int, string>(42, "test");
        Assert.Equal("42:test", result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public async Task GenericMethod_Async_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<GenericMethodService>();

        // 异步泛型方法
        var result = await proxy.AsyncGenericMethod(100);
        Assert.Equal(200, result);
    }

    #endregion

    #region 显式接口实现测试

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ExplicitInterfaceImplementation_ClassProxy_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<ExplicitImplClass>();
        var iface = (IExplicitInterface)proxy;

        // 通过接口调用显式实现的方法
        var result = iface.ExplicitMethod();
        Assert.Equal("explicit", result);
    }

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void ExplicitInterfaceImplementation_InterfaceProxyWithTarget_Should_Work(ProxyEngine engine)
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

        var implementation = new ExplicitImplClass();
        var proxy = proxyGenerator.CreateInterfaceProxy<IExplicitInterface_Proxy>(implementation);

        // 调用显式实现的方法
        var result = proxy.ExplicitMethod();
        Assert.Equal("explicit", result);

        // 验证 ImplementationMethod 正确
        var snapshot = snapshots.Single();
        Assert.Equal(typeof(ExplicitImplClass), snapshot.ImplementationMethod?.DeclaringType);
        Assert.Equal(nameof(IExplicitInterface_Proxy.ExplicitMethod), snapshot.ImplementationMethod?.Name);
    }

    #endregion

    #region 复杂场景测试

    [Theory]
    [MemberData(nameof(ProxyEngineTestSupport.Engines), MemberType = typeof(ProxyEngineTestSupport))]
    public void Complex_Generic_And_Explicit_Interface_Should_Work(ProxyEngine engine)
    {
        using var proxyGenerator = ProxyEngineTestSupport.CreateProxyGenerator(
            engine,
            configureAspect: cfg =>
            {
                cfg.Interceptors.AddDelegate((ctx, next) => ctx.Invoke(next));
            },
            strict: engine == ProxyEngine.SourceGenerator,
            allowRuntimeFallback: engine == ProxyEngine.SourceGenerator ? false : null);

        var proxy = proxyGenerator.CreateClassProxy<ComplexService>();
        var iface = (IComplexInterface)proxy;

        // 调用泛型方法
        var result = iface.GenericMethod(42);
        Assert.Equal(84, result);

        // 调用显式实现的方法
        var explicitResult = ((IComplexInterface)proxy).ExplicitMethod();
        Assert.Equal("complex-explicit", explicitResult);
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

#region 测试类型定义

// Interface Proxy with Target 测试类型
// 注意：当前 Source Generator 不支持继承接口的成员，所以直接在接口上定义方法
[AspectCoreGenerateProxy(typeof(ServiceImplementation))]
public interface IServiceWithTarget_Proxy
{
    string DoWork(string input);
}

public class ServiceImplementation : IServiceWithTarget_Proxy
{
    public string DoWork(string input) => $"{input}:processed";
}

[AspectCoreGenerateProxy(typeof(GenericServiceImplementation))]
public interface IGenericServiceWithTarget_Proxy
{
    T Process<T>(T input);
}

public class GenericServiceImplementation : IGenericServiceWithTarget_Proxy
{
    public T Process<T>(T input)
    {
        if (input is int i)
            return (T)(object)(i * 2);
        return input;
    }
}

// 泛型方法测试类型
[AspectCoreGenerateProxy]
public class GenericMethodService
{
    public virtual T SingleTypeParam<T>(T value) => value;

    public virtual TResult MultipleTypeParams<T1, TResult>(T1 input, string suffix)
        where TResult : class
        => $"{input}:{suffix}" as TResult ?? throw new InvalidOperationException();

    public virtual async Task<T> AsyncGenericMethod<T>(T value)
    {
        await Task.Delay(1);
        if (value is int i)
            return (T)(object)(i * 2);
        return value;
    }

    // 注意：带约束的泛型方法在 override 时不能重新声明约束
    // 所以这里不测试带约束的泛型方法
}

public interface IHasValue
{
    int Value { get; }
}

public class ConstraintTest : IHasValue
{
    public int Value { get; set; }
}

// 显式接口实现测试类型
public interface IExplicitInterface
{
    string ExplicitMethod();
}

// 为显式接口实现创建代理接口
[AspectCoreGenerateProxy(typeof(ExplicitImplClass))]
public interface IExplicitInterface_Proxy
{
    string ExplicitMethod();
}

[AspectCoreGenerateProxy]
public class ExplicitImplClass : IExplicitInterface, IExplicitInterface_Proxy
{
    string IExplicitInterface.ExplicitMethod() => "explicit";

    // 为代理接口提供实现
    public string ExplicitMethod() => "explicit";
}

// 复杂场景测试类型
public interface IComplexInterface
{
    T GenericMethod<T>(T input);
    string ExplicitMethod();
}

[AspectCoreGenerateProxy]
public class ComplexService : IComplexInterface
{
    public virtual T GenericMethod<T>(T input)
    {
        if (input is int i)
            return (T)(object)(i * 2);
        return input;
    }

    string IComplexInterface.ExplicitMethod() => "complex-explicit";
}

#endregion
