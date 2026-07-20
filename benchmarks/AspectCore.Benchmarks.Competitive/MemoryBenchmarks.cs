using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using CastleProxyGenerator = Castle.DynamicProxy.ProxyGenerator;

namespace AspectCore.Benchmarks.Competitive;

/// <summary>
/// Measures per-invocation memory allocation.
/// Lower allocation = less GC pressure = better throughput.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("Memory")]
public class MemoryAllocationBenchmarks
{
    private ICalculator _direct = null!;
    private ICalculator _castleProxy = null!;
    private VirtualCalculator _aspectCoreDPProxy = null!;
    private VirtualCalculator _aspectCoreSGProxy = null!;

    [GlobalSetup]
    public void Setup()
    {
        _direct = new CalculatorImpl();

        var castleGen = new CastleProxyGenerator();
        _castleProxy = castleGen.CreateInterfaceProxyWithTarget<ICalculator>(
            new CalculatorImpl(), CastlePassthroughInterceptor.Instance);

        var dpGen = CreateAspectCoreGenerator(ProxyEngine.DynamicProxy);
        _aspectCoreDPProxy = (VirtualCalculator)dpGen.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());

        var sgGen = CreateAspectCoreGenerator(ProxyEngine.SourceGenerator);
        _aspectCoreSGProxy = (VirtualCalculator)sgGen.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: 100 calls")]
    public int Direct_100Calls()
    {
        int sum = 0;
        for (int i = 0; i < 100; i++)
            sum += _direct.Add(i, i);
        return sum;
    }

    [Benchmark(Description = "Castle: 100 calls")]
    public int Castle_100Calls()
    {
        int sum = 0;
        for (int i = 0; i < 100; i++)
            sum += _castleProxy.Add(i, i);
        return sum;
    }

    [Benchmark(Description = "AspectCore-DP: 100 calls")]
    public int AspectCore_DP_100Calls()
    {
        int sum = 0;
        for (int i = 0; i < 100; i++)
            sum += _aspectCoreDPProxy.Add(i, i);
        return sum;
    }

    [Benchmark(Description = "AspectCore-SG: 100 calls")]
    public int AspectCore_SG_100Calls()
    {
        int sum = 0;
        for (int i = 0; i < 100; i++)
            sum += _aspectCoreSGProxy.Add(i, i);
        return sum;
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

/// <summary>
/// Measures proxy creation allocation (memory cost of creating proxy objects).
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("Memory", "Creation")]
public class ProxyCreationMemoryBenchmarks
{
    private CastleProxyGenerator _castleGen = null!;
    private IProxyGenerator _aspectCoreDPGen = null!;
    private IProxyGenerator _aspectCoreSGGen = null!;

    [GlobalSetup]
    public void Setup()
    {
        _castleGen = new CastleProxyGenerator();
        _aspectCoreDPGen = CreateAspectCoreGenerator(ProxyEngine.DynamicProxy);
        _aspectCoreSGGen = CreateAspectCoreGenerator(ProxyEngine.SourceGenerator);
    }

    [Benchmark(Baseline = true, Description = "Direct: new CalculatorImpl()")]
    public object Direct() => new CalculatorImpl();

    [Benchmark(Description = "Castle: CreateInterfaceProxy")]
    public object Castle_Create()
    {
        return _castleGen.CreateInterfaceProxyWithTarget<ICalculator>(
            new CalculatorImpl(), CastlePassthroughInterceptor.Instance);
    }

    [Benchmark(Description = "AspectCore-DP: CreateClassProxy")]
    public object AspectCore_DP_Create()
    {
        return _aspectCoreDPGen.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());
    }

    [Benchmark(Description = "AspectCore-SG: CreateClassProxy")]
    public object AspectCore_SG_Create()
    {
        return _aspectCoreSGGen.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());
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
