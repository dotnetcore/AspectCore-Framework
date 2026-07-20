using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;

namespace AspectCore.Benchmarks;

/// <summary>
/// Cold-start / first-call benchmarks measuring the latency of the first proxy creation
/// and first method invocation. Critical for serverless and NativeAOT scenarios where
/// startup time dominates overall performance.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("ColdStart")]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 20)]
public class ColdStartBenchmarks
{
    /// <summary>
    /// Measures first-time proxy generator creation + first proxy instance creation + first call
    /// for DynamicProxy. This includes IL emit type generation on first access.
    /// </summary>
    [Benchmark(Baseline = true, Description = "DP: First generator + proxy + call")]
    public int DynamicProxy_ColdStart()
    {
        var gen = CreateGenerator(ProxyEngine.DynamicProxy);
        var proxy = (SyncService)gen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
        return proxy.Add(1, 2);
    }

    /// <summary>
    /// Measures first-time proxy generator creation + first proxy instance creation + first call
    /// for SourceGenerator. This includes registry lookup on first access.
    /// </summary>
    [Benchmark(Description = "SG: First generator + proxy + call")]
    public int SourceGen_ColdStart()
    {
        var gen = CreateGenerator(ProxyEngine.SourceGenerator);
        var proxy = (SyncService)gen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
        return proxy.Add(1, 2);
    }

    /// <summary>
    /// Baseline: direct instantiation + call with no proxy infrastructure.
    /// </summary>
    [Benchmark(Description = "Direct: new + call (no proxy)")]
    public int Direct_ColdStart()
    {
        var svc = new SyncService();
        return svc.Add(1, 2);
    }

    private static IProxyGenerator CreateGenerator(ProxyEngine engine)
    {
        var options = new ProxyEngineOptions
        {
            Engine = engine,
            Strict = engine == ProxyEngine.SourceGenerator,
            AllowRuntimeFallback = engine == ProxyEngine.SourceGenerator ? false : null,
        };

        var builder = new ProxyGeneratorBuilder();
        builder.Configure(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*Service*"));
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
/// Measures first-call latency only (proxy generator is pre-built, but the proxy instance
/// and first invocation are included). Simulates the scenario where the DI container is warm
/// but a specific service type is resolved for the first time.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("ColdStart", "FirstCall")]
[SimpleJob(RunStrategy.ColdStart, iterationCount: 20)]
public class FirstCallBenchmarks
{
    private IProxyGenerator _dpGen = null!;
    private IProxyGenerator _sgGen = null!;

    [IterationSetup]
    public void IterationSetup()
    {
        // Recreate generators each iteration to simulate cold proxy type caches
        _dpGen = CreateGenerator(ProxyEngine.DynamicProxy);
        _sgGen = CreateGenerator(ProxyEngine.SourceGenerator);
    }

    [Benchmark(Baseline = true, Description = "DP: First proxy creation + call")]
    public int DynamicProxy_FirstCall()
    {
        var proxy = (SyncService)_dpGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
        return proxy.Add(1, 2);
    }

    [Benchmark(Description = "SG: First proxy creation + call")]
    public int SourceGen_FirstCall()
    {
        var proxy = (SyncService)_sgGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
        return proxy.Add(1, 2);
    }

    private static IProxyGenerator CreateGenerator(ProxyEngine engine)
    {
        var options = new ProxyEngineOptions
        {
            Engine = engine,
            Strict = engine == ProxyEngine.SourceGenerator,
            AllowRuntimeFallback = engine == ProxyEngine.SourceGenerator ? false : null,
        };

        var builder = new ProxyGeneratorBuilder();
        builder.Configure(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*Service*"));
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
