using System;
using System.Collections.Generic;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Seventh batch of E2E coverage tests — exercises covariant return type handling
/// in the proxy generation pipeline (TypeExtensions, MethodInfoExtensions,
/// ClassProxyAstBuilder, InterfaceImplAstBuilder). Covers: simple covariant returns,
/// generic interfaces with covariant returns, deep inheritance with covariant
/// overrides, multiple covariant methods on one type, and covariant returns with
/// generic method parameters.
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios7
{
    // ========================================================================
    // Simple covariant return — already partially covered, here we add more
    // complex inheritance and multiple methods.
    // ========================================================================

    [Fact]
    public void CovariantReturn_DeepInheritance_ProxyPreservesDerivedType()
    {
        using var host = new TestHost();
        host.Add<DeepCovariantService>();

        var service = host.Resolve<DeepCovariantService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(DeepCovariantService)));
        });

        // Three-level inheritance: BaseResult -> MidResult -> DeepResult
        var result = service.Get();
        Assert.IsType<DeepResult>(result);
        Assert.Equal("deep", result.Name);
        Assert.Equal(99, ((DeepResult)result).Extra);
        Assert.Equal("deep-specific", ((DeepResult)result).DeepValue);
    }

    [Fact]
    public void CovariantReturn_MultipleMethods_AllWorkThroughProxy()
    {
        using var host = new TestHost();
        host.Add<MultiCovariantService>();

        var service = host.Resolve<MultiCovariantService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(MultiCovariantService)));
        });

        var result1 = service.GetFirst();
        Assert.IsType<DerivedResult>(result1);
        Assert.Equal("first", result1.Name);

        var result2 = service.GetSecond();
        Assert.IsType<DeepResult>(result2);
        Assert.Equal("second", result2.Name);
    }

    [Fact]
    public void CovariantReturn_GenericInterface_ProxyWorks()
    {
        using var host = new TestHost();
        host.Add<IGenericCovariantService<string>, GenericCovariantServiceImpl>();

        var service = host.Resolve<IGenericCovariantService<string>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericCovariantService<string>)));
        });

        var result = service.Get();
        Assert.IsType<DerivedResult>(result);
        Assert.Equal("generic-impl", result.Name);
    }

    [Fact]
    public void CovariantReturn_GenericMethodWithCovariantReturn_Works()
    {
        using var host = new TestHost();
        host.Add<GenericMethodCovariantService>();

        var service = host.Resolve<GenericMethodCovariantService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(GenericMethodCovariantService)));
        });

        var result = service.Get<DerivedResult>();
        Assert.IsType<DerivedResult>(result);
        Assert.Equal("generic-method", result.Name);
    }

    [Fact]
    public void CovariantReturn_InterfaceWithCovariantGenericParameter_Works()
    {
        using var host = new TestHost();
        host.Add<ICovariantRepo<DerivedResult>, CovariantRepoImpl>();

        var service = host.Resolve<ICovariantRepo<DerivedResult>>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICovariantRepo<DerivedResult>)));
        });

        var result = service.Get();
        Assert.IsType<DerivedResult>(result);
        Assert.Equal("covariant-repo", result.Name);
    }

    [Fact]
    public void CovariantReturn_InterceptorCanAccessReturnValue()
    {
        using var host = new TestHost();
        host.Add<DeepCovariantService>();

        InterceptorLog.Clear();
        var service = host.Resolve<DeepCovariantService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Covariant.Before");
                await ctx.Invoke(next);
                // The return value should be the derived type.
                var rv = ctx.ReturnValue;
                InterceptorLog.Entries.Add($"Covariant.Result={rv?.GetType().Name}");
                InterceptorLog.Entries.Add("Covariant.After");
            }, Predicates.ForService(nameof(DeepCovariantService)));
        });

        var result = service.Get();

        Assert.IsType<DeepResult>(result);
        Assert.Contains("Covariant.Before", InterceptorLog.Entries);
        Assert.Contains("Covariant.Result=DeepResult", InterceptorLog.Entries);
        Assert.Contains("Covariant.After", InterceptorLog.Entries);
    }

    [Fact]
    public void CovariantReturn_ClassProxy_WithPropertyInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<CovariantWithPropertyService>();

        var service = host.Resolve<CovariantWithPropertyService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(CovariantWithPropertyService)));
        });

        // Property getter with covariant return type.
        var result = service.Current;
        Assert.IsType<DerivedResult>(result);
        Assert.Equal("property-covariant", result.Name);
    }

    [Fact]
    public void CovariantReturn_NonCovariantOverride_AlsoWorks()
    {
        // A class that has both covariant and non-covariant overrides.
        using var host = new TestHost();
        host.Add<MixedOverrideService>();

        var service = host.Resolve<MixedOverrideService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(MixedOverrideService)));
        });

        var covariantResult = service.Get();
        Assert.IsType<DerivedResult>(covariantResult);
        Assert.Equal("mixed-covariant", covariantResult.Name);

        var nonCovariantResult = service.GetNonCovariant();
        Assert.Equal("non-covariant", nonCovariantResult);
    }

    // ========================================================================
    // Helper types for covariant return scenarios.
    // ========================================================================

    public class MidResult : BaseResult
    {
        public string MidValue { get; set; } = "mid";
    }

    public class DeepResult : MidResult
    {
        public string DeepValue { get; set; } = "deep";
        public int Extra { get; set; }
    }

    /// <summary>
    /// Three-level inheritance with covariant return at each level.
    /// </summary>
    public class DeepCovariantServiceBase
    {
        public virtual BaseResult Get() => new() { Name = "base" };
    }

    public class DeepCovariantServiceMid : DeepCovariantServiceBase
    {
        public override MidResult Get() => new() { Name = "mid", MidValue = "mid-specific" };
    }

    public class DeepCovariantService : DeepCovariantServiceMid
    {
        public override DeepResult Get() => new()
        {
            Name = "deep",
            MidValue = "mid-specific",
            DeepValue = "deep-specific",
            Extra = 99
        };
    }

    /// <summary>
    /// Multiple covariant return methods on one type.
    /// </summary>
    public class MultiCovariantServiceBase
    {
        public virtual BaseResult GetFirst() => new() { Name = "first-base" };
        public virtual BaseResult GetSecond() => new() { Name = "second-base" };
    }

    public class MultiCovariantService : MultiCovariantServiceBase
    {
        public override DerivedResult GetFirst() => new() { Name = "first", Extra = 1 };
        public override DeepResult GetSecond() => new() { Name = "second", DeepValue = "s2", Extra = 2 };
    }

    /// <summary>
    /// Generic interface with a method that returns a base result type.
    /// The implementation returns the base type (no covariant return for interfaces).
    /// </summary>
    public interface IGenericCovariantService<T>
    {
        BaseResult Get();
        T Echo(T value);
    }

    public class GenericCovariantServiceImpl : IGenericCovariantService<string>
    {
        public BaseResult Get() => new DerivedResult { Name = "generic-impl", Extra = 42 };
        public string Echo(string value) => value;
    }

    /// <summary>
    /// Generic method with covariant return.
    /// </summary>
    public class GenericMethodCovariantServiceBase
    {
        public virtual BaseResult Get<T>() where T : BaseResult, new() => new T() { Name = "base-generic" };
    }

    public class GenericMethodCovariantService : GenericMethodCovariantServiceBase
    {
        public override T Get<T>() => new T() { Name = "generic-method" };
    }

    /// <summary>
    /// Interface with covariant generic parameter (out T).
    /// </summary>
    public interface ICovariantRepo<out T> where T : BaseResult
    {
        T Get();
    }

    public class CovariantRepoImpl : ICovariantRepo<DerivedResult>
    {
        public DerivedResult Get() => new() { Name = "covariant-repo", Extra = 7 };
    }

    /// <summary>
    /// Class with a property that has a covariant return type.
    /// </summary>
    public class CovariantWithPropertyServiceBase
    {
        public virtual BaseResult Current => new() { Name = "base-property" };
    }

    public class CovariantWithPropertyService : CovariantWithPropertyServiceBase
    {
        public override DerivedResult Current => new() { Name = "property-covariant", Extra = 5 };
    }

    /// <summary>
    /// Class with both covariant and non-covariant overrides.
    /// </summary>
    public class MixedOverrideServiceBase
    {
        public virtual BaseResult Get() => new() { Name = "base" };
        public virtual string GetNonCovariant() => "base-string";
    }

    public class MixedOverrideService : MixedOverrideServiceBase
    {
        public override DerivedResult Get() => new() { Name = "mixed-covariant", Extra = 10 };
        public override string GetNonCovariant() => "non-covariant";
    }
}
