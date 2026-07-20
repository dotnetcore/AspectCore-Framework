using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Benchmarks;

/// <summary>
/// Benchmarks the overhead of interceptor chain length.
/// Measures how performance scales with 1, 2, 5, and 10 interceptors in the pipeline.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("Pipeline")]
public class PipelineBenchmarks
{
    private SyncService _direct = null!;

    private SyncService _dp1 = null!;
    private SyncService _dp2 = null!;
    private SyncService _dp5 = null!;
    private SyncService _dp10 = null!;

    private SyncService _sg1 = null!;
    private SyncService _sg2 = null!;
    private SyncService _sg5 = null!;
    private SyncService _sg10 = null!;

    [GlobalSetup]
    public void Setup()
    {
        _direct = new SyncService();

        _dp1 = CreateProxy(ProxyEngine.DynamicProxy, 1);
        _dp2 = CreateProxy(ProxyEngine.DynamicProxy, 2);
        _dp5 = CreateProxy(ProxyEngine.DynamicProxy, 5);
        _dp10 = CreateProxy(ProxyEngine.DynamicProxy, 10);

        _sg1 = CreateProxy(ProxyEngine.SourceGenerator, 1);
        _sg2 = CreateProxy(ProxyEngine.SourceGenerator, 2);
        _sg5 = CreateProxy(ProxyEngine.SourceGenerator, 5);
        _sg10 = CreateProxy(ProxyEngine.SourceGenerator, 10);
    }

    private static SyncService CreateProxy(ProxyEngine engine, int interceptorCount)
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
            for (int i = 0; i < interceptorCount; i++)
            {
                cfg.Interceptors.AddTyped<PassthroughInterceptor>(Predicates.ForService("*Service*"));
            }
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

        var gen = builder.Build();
        return (SyncService)gen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
    }

    // ── Baseline ────────────────────────────────────────────────────────

    [Benchmark(Baseline = true, Description = "Direct (no interceptor)")]
    public int Direct() => _direct.Add(1, 2);

    // ── DynamicProxy pipeline depth ─────────────────────────────────────

    [Benchmark(Description = "DP: 1 interceptor")]
    public int DP_1() => _dp1.Add(1, 2);

    [Benchmark(Description = "DP: 2 interceptors")]
    public int DP_2() => _dp2.Add(1, 2);

    [Benchmark(Description = "DP: 5 interceptors")]
    public int DP_5() => _dp5.Add(1, 2);

    [Benchmark(Description = "DP: 10 interceptors")]
    public int DP_10() => _dp10.Add(1, 2);

    // ── SourceGenerator pipeline depth ──────────────────────────────────

    [Benchmark(Description = "SG: 1 interceptor")]
    public int SG_1() => _sg1.Add(1, 2);

    [Benchmark(Description = "SG: 2 interceptors")]
    public int SG_2() => _sg2.Add(1, 2);

    [Benchmark(Description = "SG: 5 interceptors")]
    public int SG_5() => _sg5.Add(1, 2);

    [Benchmark(Description = "SG: 10 interceptors")]
    public int SG_10() => _sg10.Add(1, 2);
}

/// <summary>
/// A minimal pass-through interceptor used for pipeline depth benchmarks.
/// Does no work — measures pure pipeline dispatch overhead.
/// </summary>
public sealed class PassthroughInterceptor : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        return next(context);
    }
}
