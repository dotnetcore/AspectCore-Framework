using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;
using Castle.DynamicProxy;
using CastleProxyGenerator = Castle.DynamicProxy.ProxyGenerator;

namespace AspectCore.Benchmarks.Competitive;

/// <summary>
/// Measures steady-state overhead: repeated method calls through a pre-created proxy.
/// This represents the typical runtime cost after warm-up.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("SteadyState", "Sync")]
public class SteadyStateSyncBenchmarks
{
    private ICalculator _direct = null!;
    private ICalculator _castleProxy = null!;
    private VirtualCalculator _aspectCoreDPProxy = null!;
    private VirtualCalculator _aspectCoreSGProxy = null!;

    [GlobalSetup]
    public void Setup()
    {
        _direct = new CalculatorImpl();

        // Castle proxy
        var castleGen = new CastleProxyGenerator();
        _castleProxy = castleGen.CreateInterfaceProxyWithTarget<ICalculator>(
            new CalculatorImpl(), CastlePassthroughInterceptor.Instance);

        // AspectCore DynamicProxy
        var dpGen = CreateAspectCoreGenerator(ProxyEngine.DynamicProxy);
        _aspectCoreDPProxy = (VirtualCalculator)dpGen.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());

        // AspectCore SourceGenerator
        var sgGen = CreateAspectCoreGenerator(ProxyEngine.SourceGenerator);
        _aspectCoreSGProxy = (VirtualCalculator)sgGen.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct call")]
    public int Direct() => _direct.Add(1, 2);

    [Benchmark(Description = "Castle DynamicProxy")]
    public int Castle() => _castleProxy.Add(1, 2);

    [Benchmark(Description = "AspectCore DynamicProxy")]
    public int AspectCore_DP() => _aspectCoreDPProxy.Add(1, 2);

    [Benchmark(Description = "AspectCore SourceGenerator")]
    public int AspectCore_SG() => _aspectCoreSGProxy.Add(1, 2);

    [Benchmark(Description = "Direct: Concat")]
    public string Direct_Concat() => _direct.Concat("hello", "world");

    [Benchmark(Description = "Castle: Concat")]
    public string Castle_Concat() => _castleProxy.Concat("hello", "world");

    [Benchmark(Description = "AspectCore-DP: Concat")]
    public string AspectCore_DP_Concat() => _aspectCoreDPProxy.Concat("hello", "world");

    [Benchmark(Description = "AspectCore-SG: Concat")]
    public string AspectCore_SG_Concat() => _aspectCoreSGProxy.Concat("hello", "world");

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
/// Measures steady-state overhead for async methods.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("SteadyState", "Async")]
public class SteadyStateAsyncBenchmarks
{
    private IAsyncCalculator _direct = null!;
    private IAsyncCalculator _castleProxy = null!;
    private VirtualAsyncCalculator _aspectCoreDPProxy = null!;
    private VirtualAsyncCalculator _aspectCoreSGProxy = null!;

    [GlobalSetup]
    public void Setup()
    {
        _direct = new AsyncCalculatorImpl();

        // Castle proxy (note: Castle has no native async interception)
        var castleGen = new CastleProxyGenerator();
        _castleProxy = castleGen.CreateInterfaceProxyWithTarget<IAsyncCalculator>(
            new AsyncCalculatorImpl(), CastlePassthroughInterceptor.Instance);

        // AspectCore DynamicProxy
        var dpGen = CreateAspectCoreGenerator(ProxyEngine.DynamicProxy);
        _aspectCoreDPProxy = (VirtualAsyncCalculator)dpGen.CreateClassProxy(
            typeof(VirtualAsyncCalculator), typeof(VirtualAsyncCalculator), Array.Empty<object>());

        // AspectCore SourceGenerator
        var sgGen = CreateAspectCoreGenerator(ProxyEngine.SourceGenerator);
        _aspectCoreSGProxy = (VirtualAsyncCalculator)sgGen.CreateClassProxy(
            typeof(VirtualAsyncCalculator), typeof(VirtualAsyncCalculator), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: Task<int>")]
    public Task<int> Direct_Task() => _direct.AddAsync(1, 2);

    [Benchmark(Description = "Castle: Task<int>")]
    public Task<int> Castle_Task() => _castleProxy.AddAsync(1, 2);

    [Benchmark(Description = "AspectCore-DP: Task<int>")]
    public Task<int> AspectCore_DP_Task() => _aspectCoreDPProxy.AddAsync(1, 2);

    [Benchmark(Description = "AspectCore-SG: Task<int>")]
    public Task<int> AspectCore_SG_Task() => _aspectCoreSGProxy.AddAsync(1, 2);

    [Benchmark(Description = "Direct: ValueTask<int>")]
    public ValueTask<int> Direct_ValueTask() => _direct.MultiplyAsync(3, 4);

    [Benchmark(Description = "Castle: ValueTask<int> (no native support)")]
    public ValueTask<int> Castle_ValueTask() => _castleProxy.MultiplyAsync(3, 4);

    [Benchmark(Description = "AspectCore-DP: ValueTask<int>")]
    public ValueTask<int> AspectCore_DP_ValueTask() => _aspectCoreDPProxy.MultiplyAsync(3, 4);

    [Benchmark(Description = "AspectCore-SG: ValueTask<int>")]
    public ValueTask<int> AspectCore_SG_ValueTask() => _aspectCoreSGProxy.MultiplyAsync(3, 4);

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
