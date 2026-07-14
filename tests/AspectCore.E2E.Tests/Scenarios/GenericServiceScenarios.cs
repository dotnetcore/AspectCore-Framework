using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for generic service scenarios: generic interfaces with generic
/// methods, multiple type parameters, generic constraints, DI resolution of
/// generic services with interceptors, and generic methods with ref/out
/// parameters. Real DI container, real proxies — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class GenericServiceScenarios
{
    [Fact]
    public void GenericInterface_GenericMethodProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IGenericRepository<BaseResult>, GenericRepository<BaseResult>>();

        var service = host.Resolve<IGenericRepository<BaseResult>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericRepository<BaseResult>)));
        });

        // Generic method Transform takes a Func<T, TResult>.
        var result = service.Transform(new BaseResult { Name = "test" }, r => r.Name.ToUpper());

        Assert.Equal("TEST", result);
        Assert.IsNotType<GenericRepository<BaseResult>>(service);
    }

    [Fact]
    public void GenericInterface_GetAll_ReturnsItems()
    {
        using var host = new TestHost();
        host.Add<IGenericRepository<BaseResult>, GenericRepository<BaseResult>>();

        var service = host.Resolve<IGenericRepository<BaseResult>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericRepository<BaseResult>)));
        });

        var all = service.GetAll().ToList();

        Assert.Equal(2, all.Count);
        Assert.All(all, item => Assert.NotNull(item));
    }

    [Fact]
    public void GenericService_MultipleTypeParameters_Works()
    {
        using var host = new TestHost();
        host.Add<IPairService<string, int>, PairService<string, int>>();

        var service = host.Resolve<IPairService<string, int>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IPairService<string, int>)));
        });

        var pair = service.CreatePair("answer", 42);

        Assert.Equal("answer", service.GetKey(pair));
        Assert.Equal(42, service.GetValue(pair));
        Assert.IsNotType<PairService<string, int>>(service);
    }

    [Fact]
    public void GenericService_WithConstraints_ResolvedFromDI_Works()
    {
        // GenericRepository<T> has constraints: where T : class, new()
        using var host = new TestHost();
        host.Add<IGenericRepository<BaseResult>, GenericRepository<BaseResult>>();

        var service = host.Resolve<IGenericRepository<BaseResult>>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add($"Generic.{ctx.ServiceMethod.Name}");
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(IGenericRepository<BaseResult>)));
        });

        InterceptorLog.Clear();
        var item = service.GetById(1);

        Assert.NotNull(item);
        Assert.IsType<BaseResult>(item);
        Assert.Contains("Generic.GetById", InterceptorLog.Entries);
    }

    [Fact]
    public void GenericMethod_WithRefParameter_Works()
    {
        using var host = new TestHost();
        host.Add<IGenericParameterService<string>, GenericParameterService<string>>();

        var service = host.Resolve<IGenericParameterService<string>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericParameterService<string>)));
        });

        var value = "hello";
        service.EchoRef(ref value);

        Assert.Equal("hello", value);
        Assert.IsNotType<GenericParameterService<string>>(service);
    }

    [Fact]
    public void GenericMethod_WithOutParameter_Works()
    {
        using var host = new TestHost();
        host.Add<IGenericParameterService<int>, GenericParameterService<int>>();

        var service = host.Resolve<IGenericParameterService<int>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericParameterService<int>)));
        });

        service.GetOutput(out var output);

        Assert.Equal(0, output);
    }

    [Fact]
    public void GenericMethod_SwapRefParameters_Works()
    {
        using var host = new TestHost();
        host.Add<IGenericParameterService<int>, GenericParameterService<int>>();

        var service = host.Resolve<IGenericParameterService<int>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericParameterService<int>)));
        });

        var first = 10;
        var second = 20;
        var result = service.Swap(ref first, ref second);

        // After swap: first=20, second=10; result is the new first (20).
        Assert.Equal(20, first);
        Assert.Equal(10, second);
        Assert.Equal(20, result);
    }

    [Fact]
    public void GenericService_InterceptorExecutes_OnEveryCall()
    {
        using var host = new TestHost();
        host.Add<IGenericRepository<BaseResult>, GenericRepository<BaseResult>>();

        InterceptorLog.Clear();
        var service = host.Resolve<IGenericRepository<BaseResult>>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add($"Intercepted:{ctx.ServiceMethod.Name}");
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(IGenericRepository<BaseResult>)));
        });

        service.GetById(1);
        service.GetAll().ToList();
        service.Transform(new BaseResult(), r => r.Name);

        Assert.Contains("Intercepted:GetById", InterceptorLog.Entries);
        Assert.Contains("Intercepted:GetAll", InterceptorLog.Entries);
        Assert.Contains("Intercepted:Transform", InterceptorLog.Entries);
    }
}
