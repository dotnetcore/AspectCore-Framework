using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using CastleProxyGenerator = Castle.DynamicProxy.ProxyGenerator;

namespace AspectCore.Benchmarks.Competitive;

/// <summary>
/// Measures first-invocation latency: proxy creation + first interception call.
/// This is the "cold start" cost that users experience on application startup.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("FirstInvocation")]
public class FirstInvocationBenchmarks
{
    private CastleProxyGenerator _castleGenerator = null!;
    private IProxyGenerator _aspectCoreDPGenerator = null!;
    private IProxyGenerator _aspectCoreSGGenerator = null!;

    [GlobalSetup]
    public void Setup()
    {
        _castleGenerator = new CastleProxyGenerator();
        _aspectCoreDPGenerator = CreateAspectCoreGenerator(ProxyEngine.DynamicProxy);
        _aspectCoreSGGenerator = CreateAspectCoreGenerator(ProxyEngine.SourceGenerator);
    }

    [Benchmark(Baseline = true, Description = "Direct: new + call")]
    public int Direct()
    {
        var calc = new CalculatorImpl();
        return calc.Add(1, 2);
    }

    [Benchmark(Description = "Castle: CreateInterfaceProxy + call")]
    public int Castle_InterfaceProxy()
    {
        var proxy = _castleGenerator.CreateInterfaceProxyWithTarget<ICalculator>(
            new CalculatorImpl(), CastlePassthroughInterceptor.Instance);
        return proxy.Add(1, 2);
    }

    [Benchmark(Description = "Castle: CreateClassProxy + call")]
    public int Castle_ClassProxy()
    {
        var proxy = _castleGenerator.CreateClassProxy<VirtualCalculator>(
            CastlePassthroughInterceptor.Instance);
        return proxy.Add(1, 2);
    }

    [Benchmark(Description = "AspectCore-DP: CreateClassProxy + call")]
    public int AspectCore_DP_ClassProxy()
    {
        var proxy = (VirtualCalculator)_aspectCoreDPGenerator.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());
        return proxy.Add(1, 2);
    }

    [Benchmark(Description = "AspectCore-SG: CreateClassProxy + call")]
    public int AspectCore_SG_ClassProxy()
    {
        var proxy = (VirtualCalculator)_aspectCoreSGGenerator.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());
        return proxy.Add(1, 2);
    }

    [Benchmark(Description = "AspectCore-DP: CreateInterfaceProxy + call")]
    public int AspectCore_DP_InterfaceProxy()
    {
        var proxy = (ICalculator)_aspectCoreDPGenerator.CreateInterfaceProxy(
            typeof(ICalculator), new CalculatorImpl());
        return proxy.Add(1, 2);
    }

    [Benchmark(Description = "AspectCore-SG: CreateInterfaceProxy + call")]
    public int AspectCore_SG_InterfaceProxy()
    {
        var proxy = (ICalculator)_aspectCoreSGGenerator.CreateInterfaceProxy(
            typeof(ICalculator), new CalculatorImpl());
        return proxy.Add(1, 2);
    }

    private static IProxyGenerator CreateAspectCoreGenerator(ProxyEngine engine)
    {
        var options = new ProxyEngineOptions
        {
            Engine = engine,
            Strict = engine == ProxyEngine.SourceGenerator,
            AllowRuntimeFallback = engine == ProxyEngine.SourceGenerator ? false : (bool?)null,
        };

        var builder = new ProxyGeneratorBuilder();
        builder.Configure(cfg =>
        {
            cfg.Interceptors.AddTyped<AspectCorePassthroughInterceptor>(
                Predicates.ForService("*Calculator*"));
        });
        builder.ConfigureService(sc =>
        {
            sc.AddInstance(options);
            if (engine != ProxyEngine.DynamicProxy)
            {
                sc.RemoveAll(typeof(IProxyTypeGenerator));
                sc.AddInstance<IProxyTypeGenerator>(
                    new SourceGeneratedProxyTypeGenerator(
                        new AspectValidatorBuilder(sc.Configuration),
                        options,
                        Array.Empty<ISourceGeneratedProxyRegistry>()));
            }
        });
        return builder.Build();
    }
}
