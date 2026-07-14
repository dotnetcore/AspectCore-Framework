using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for parameter handling in proxied methods: ref/out parameters,
/// generic methods, default values, nullable types, optional parameters, and
/// multi-parameter methods. All tests run through the real proxy pipeline with
/// interceptors configured — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ParameterHandlingScenarios
{
    [Fact]
    public void RefParameter_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Ref.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Ref.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var value = 10;
        service.Increment(ref value);

        Assert.Equal(11, value);
        Assert.Contains("Ref.Before", InterceptorLog.Entries);
        Assert.Contains("Ref.After", InterceptorLog.Entries);
    }

    [Fact]
    public void OutParameter_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("Out.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("Out.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        service.GetOutput(7, out var doubled);

        Assert.Equal(14, doubled);
        Assert.Contains("Out.Before", InterceptorLog.Entries);
        Assert.Contains("Out.After", InterceptorLog.Entries);
    }

    [Fact]
    public void GenericMethodParameter_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Generic method with value type.
        Assert.Equal(42, service.Echo(42));

        // Generic method with reference type.
        Assert.Equal("hello", service.Echo("hello"));

        // Generic method with a complex object.
        var result = service.Echo(new BaseResult { Name = "test" });
        Assert.Equal("test", result.Name);
    }

    [Fact]
    public void DefaultParameterValues_ArePreserved_ThroughProxy()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Default value "!" is used when second argument is omitted.
        Assert.Equal("hi!", service.Concat("hi"));

        // Explicit value overrides the default.
        Assert.Equal("hi?", service.Concat("hi", "?"));
    }

    [Fact]
    public void NullableValueTypeParameter_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterService, ParameterService>();

        var service = host.Resolve<IParameterService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IParameterService)));
        });

        // Nullable int with a value.
        Assert.Equal(42, service.NullableInt(42));

        // Nullable int with null.
        Assert.Null(service.NullableInt(null));

        // Nullable bool with a value.
        Assert.True(service.NullableBool(true));

        // Nullable bool with null.
        Assert.Null(service.NullableBool(null));
    }

    [Fact]
    public void NullableReferenceTypeParameter_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        // Nullable reference type with null.
        Assert.Equal("Hello, stranger", service.Greet(null));

        // Nullable reference type with a value.
        Assert.Equal("Hello, Alice", service.Greet("Alice"));
    }

    [Fact]
    public void OptionalParameters_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterService, ParameterService>();

        var service = host.Resolve<IParameterService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IParameterService)));
        });

        // All defaults used.
        Assert.Equal("required-default-42", service.OptionalParameters("required"));

        // Override first optional.
        Assert.Equal("required-custom-42", service.OptionalParameters("required", "custom"));

        // Override all.
        Assert.Equal("required-custom-99", service.OptionalParameters("required", "custom", 99));
    }

    [Fact]
    public void MultipleParametersOfDifferentTypes_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterService, ParameterService>();

        var service = host.Resolve<IParameterService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IParameterService)));
        });

        var result = service.MultipleParameters(10, "hello", 3.14, true);

        Assert.Equal("10-hello-3.14-True", result);
    }

    [Fact]
    public void MultipleRefAndOutParameters_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterService, ParameterService>();

        var service = host.Resolve<IParameterService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IParameterService)));
        });

        var refVal = 5;
        service.MultipleRefAndOut(ref refVal, out var outVal);

        Assert.Equal(10, refVal);
        Assert.Equal(15, outVal);
    }

    /// <summary>
    /// Service interface with various parameter scenarios not covered by ICalculatorService.
    /// </summary>
    public interface IParameterService
    {
        int? NullableInt(int? value);
        bool? NullableBool(bool? value);
        string OptionalParameters(string required, string optional = "default", int count = 42);
        string MultipleParameters(int a, string b, double c, bool d);
        void MultipleRefAndOut(ref int refVal, out int outVal);
    }

    /// <summary>
    /// Real implementation of IParameterService — no mocks.
    /// </summary>
    public class ParameterService : IParameterService
    {
        public int? NullableInt(int? value) => value;

        public bool? NullableBool(bool? value) => value;

        public string OptionalParameters(string required, string optional = "default", int count = 42)
            => $"{required}-{optional}-{count}";

        public string MultipleParameters(int a, string b, double c, bool d)
            => $"{a}-{b}-{c}-{d}";

        public void MultipleRefAndOut(ref int refVal, out int outVal)
        {
            refVal *= 2;
            outVal = refVal + 5;
        }
    }
}
