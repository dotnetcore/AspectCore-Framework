using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for the direct ProxyGenerator API: ProxyGeneratorBuilder,
/// IProxyGenerator.CreateClassProxy/CreateInterfaceProxy, and
/// ProxyGeneratorExtensions. Real proxy generation, no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class DirectProxyGeneratorScenarios
{
    [Fact]
    public void ProxyGeneratorBuilder_Build_ProducesWorkingGenerator()
    {
        var builder = new ProxyGeneratorBuilder();
        var generator = builder.Build();

        Assert.NotNull(generator);

        // The generator must create a working class proxy.
        var proxy = generator.CreateClassProxy<CalculatorService>();
        Assert.NotNull(proxy);
        Assert.Equal(7, proxy.Add(3, 4));
    }

    [Fact]
    public void ProxyGeneratorBuilder_WithConfiguration_ProducesWorkingProxy()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(CalculatorService)));
        });
        var generator = builder.Build();

        // The generator produces a working class proxy even with configuration.
        var proxy = generator.CreateClassProxy<CalculatorService>();
        Assert.NotNull(proxy);
        Assert.IsNotType<CalculatorService>(proxy);
        Assert.Equal(7, proxy.Add(3, 4));
        Assert.Equal("hi!", proxy.Concat("hi"));
    }

    [Fact]
    public void CreateClassProxy_GenericMethod_Works()
    {
        var builder = new ProxyGeneratorBuilder();
        var generator = builder.Build();

        var proxy = generator.CreateClassProxy<CalculatorService>();
        Assert.NotNull(proxy);

        Assert.Equal(42, proxy.Echo(42));
        Assert.Equal("hello", proxy.Echo("hello"));
    }

    [Fact]
    public async Task CreateClassProxy_AsyncMethod_Works()
    {
        var builder = new ProxyGeneratorBuilder();
        var generator = builder.Build();

        var proxy = generator.CreateClassProxy<CalculatorService>();
        Assert.NotNull(proxy);

        var product = await proxy.MultiplyAsync(3, 4);
        Assert.Equal(12, product);

        var quotient = await proxy.DivideAsync(10, 2);
        Assert.Equal(5, quotient);
    }

    [Fact]
    public void CreateClassProxy_RefAndOutParameters_Work()
    {
        var builder = new ProxyGeneratorBuilder();
        var generator = builder.Build();

        var proxy = generator.CreateClassProxy<CalculatorService>();
        Assert.NotNull(proxy);

        var refVal = 5;
        proxy.Increment(ref refVal);
        Assert.Equal(6, refVal);

        proxy.GetOutput(7, out var outVal);
        Assert.Equal(14, outVal);
    }

    [Fact]
    public void CreateInterfaceProxy_WithoutImplementation_Works()
    {
        var builder = new ProxyGeneratorBuilder();
        builder.Configure(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });
        var generator = builder.Build();

        // Interface proxy without a concrete implementation — the proxy returns
        // default values for non-intercepted methods.
        var proxy = generator.CreateInterfaceProxy<ICalculatorService>();
        Assert.NotNull(proxy);
        Assert.IsAssignableFrom<ICalculatorService>(proxy);
    }

    [Fact]
    public void CreateInterfaceProxy_WithImplementation_Works()
    {
        var builder = new ProxyGeneratorBuilder();
        var generator = builder.Build();

        // Interface proxy with a concrete implementation instance.
        var impl = new CalculatorService();
        var proxy = generator.CreateInterfaceProxy<ICalculatorService>(impl);
        Assert.NotNull(proxy);
        Assert.Equal(15, proxy.Add(7, 8));
        Assert.Equal("hi!", proxy.Concat("hi"));
    }

    [Fact]
    public void CreateClassProxy_WithConstructorArgs_Works()
    {
        var builder = new ProxyGeneratorBuilder();
        var generator = builder.Build();

        // Class proxy with constructor arguments.
        var proxy = generator.CreateClassProxy<ServiceWithCtor, ServiceWithCtor>("hello");
        Assert.NotNull(proxy);
        Assert.Equal("hello", proxy.GetMessage());
    }

    /// <summary>
    /// Service that requires a constructor argument — used to test
    /// CreateClassProxy with args.
    /// </summary>
    public class ServiceWithCtor
    {
        private readonly string _message;

        public ServiceWithCtor(string message)
        {
            _message = message;
        }

        public virtual string GetMessage() => _message;
    }
}
