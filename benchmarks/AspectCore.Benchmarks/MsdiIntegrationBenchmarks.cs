using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Benchmarks;

/// <summary>
/// Benchmarks the real-world path: service resolution through Microsoft.Extensions.DependencyInjection
/// with ConfigureDynamicProxy, comparing vanilla MSDI vs DynamicProxy engine vs SourceGenerator engine.
///
/// IMPORTANT: The SourceGenerator only generates ClassProxy types (not InterfaceProxy types).
/// Therefore, to exercise the actual SG-generated proxy code path that uses
/// SourceGeneratedAspectContext + IAspectInvokeDelegate, we register services as concrete
/// classes (AddTransient&lt;SampleMsdiServiceImpl&gt;). This mirrors how class-based proxying
/// works in production NativeAOT scenarios.
///
/// Context verification:
/// - DynamicProxy path uses RuntimeAspectContext (via AspectActivator + MethodReflector)
/// - SourceGenerator path uses SourceGeneratedAspectContext (via IAspectInvokeDelegate)
/// Both are verified at setup time to ensure we're measuring the correct code paths.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("MSDI")]
public class MsdiIntegrationBenchmarks
{
    private ServiceProvider _vanillaProvider = null!;
    private ServiceProvider _dynamicProxyProvider = null!;
    private ServiceProvider _sourceGenProvider = null!;

    // Singleton providers kept alive for hot-path-only benchmarks
    private ServiceProvider _vanillaSingletonProvider = null!;
    private ServiceProvider _dpSingletonProvider = null!;
    private ServiceProvider _sgSingletonProvider = null!;

    // Pre-resolved singleton instances for hot-path-only benchmarks
    private SampleMsdiServiceImpl _vanillaSingleton = null!;
    private SampleMsdiServiceImpl _dpSingleton = null!;
    private SampleMsdiServiceImpl _sgSingleton = null!;

    [GlobalSetup]
    public void Setup()
    {
        // ── Vanilla MSDI — no proxy at all ──────────────────────────────
        var vanillaServices = new ServiceCollection();
        vanillaServices.AddTransient<SampleMsdiServiceImpl>();
        _vanillaProvider = vanillaServices.BuildServiceProvider();

        // ── DynamicProxy engine via MSDI ────────────────────────────────
        var dpServices = new ServiceCollection();
        dpServices.AddTransient<SampleMsdiServiceImpl>();
        dpServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        _dynamicProxyProvider = dpServices.BuildDynamicProxyProvider();

        // ── SourceGenerator engine via MSDI ─────────────────────────────
        var sgServices = new ServiceCollection();
        sgServices.AddTransient<SampleMsdiServiceImpl>();
        sgServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        sgServices.ConfigureDynamicProxyEngine(opts =>
        {
            opts.Engine = ProxyEngine.SourceGenerator;
            opts.Strict = true;
            opts.AllowRuntimeFallback = false;
        });
        _sourceGenProvider = sgServices.BuildDynamicProxyProvider();

        // ── Verify context types during warmup ──────────────────────────
        VerifyContextTypes();

        // ── Build singleton providers and pre-resolve for hot-path benchmarks ──
        var vanillaSingletonServices = new ServiceCollection();
        vanillaSingletonServices.AddSingleton<SampleMsdiServiceImpl>();
        _vanillaSingletonProvider = vanillaSingletonServices.BuildServiceProvider();
        _vanillaSingleton = _vanillaSingletonProvider.GetRequiredService<SampleMsdiServiceImpl>();

        var dpSingletonServices = new ServiceCollection();
        dpSingletonServices.AddSingleton<SampleMsdiServiceImpl>();
        dpSingletonServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        _dpSingletonProvider = dpSingletonServices.BuildDynamicProxyProvider();
        _dpSingleton = _dpSingletonProvider.GetRequiredService<SampleMsdiServiceImpl>();

        var sgSingletonServices = new ServiceCollection();
        sgSingletonServices.AddSingleton<SampleMsdiServiceImpl>();
        sgSingletonServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        sgSingletonServices.ConfigureDynamicProxyEngine(opts =>
        {
            opts.Engine = ProxyEngine.SourceGenerator;
            opts.Strict = true;
            opts.AllowRuntimeFallback = false;
        });
        _sgSingletonProvider = sgSingletonServices.BuildDynamicProxyProvider();
        _sgSingleton = _sgSingletonProvider.GetRequiredService<SampleMsdiServiceImpl>();
    }

    /// <summary>
    /// Verifies that each provider path uses the expected AspectContext implementation.
    /// This is done by installing a diagnostic interceptor during warmup that captures
    /// the actual context type name.
    /// </summary>
    private void VerifyContextTypes()
    {
        // Build diagnostic providers with a context-capturing interceptor
        var dpDiagServices = new ServiceCollection();
        dpDiagServices.AddTransient<SampleMsdiServiceImpl>();
        dpDiagServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<ContextTypeCapturingInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        using var dpDiag = dpDiagServices.BuildDynamicProxyProvider();

        var sgDiagServices = new ServiceCollection();
        sgDiagServices.AddTransient<SampleMsdiServiceImpl>();
        sgDiagServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<ContextTypeCapturingInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        sgDiagServices.ConfigureDynamicProxyEngine(opts =>
        {
            opts.Engine = ProxyEngine.SourceGenerator;
            opts.Strict = true;
            opts.AllowRuntimeFallback = false;
        });
        using var sgDiag = sgDiagServices.BuildDynamicProxyProvider();

        // Invoke to capture context type
        ContextTypeCapturingInterceptor.CapturedTypeName = null;
        var dpSvc = dpDiag.GetRequiredService<SampleMsdiServiceImpl>();
        dpSvc.Compute(1, 1);
        var dpContextType = ContextTypeCapturingInterceptor.CapturedTypeName;

        ContextTypeCapturingInterceptor.CapturedTypeName = null;
        var sgSvc = sgDiag.GetRequiredService<SampleMsdiServiceImpl>();
        sgSvc.Compute(1, 1);
        var sgContextType = ContextTypeCapturingInterceptor.CapturedTypeName;

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║  MSDI Integration Benchmark — Context Type Verification         ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║  DynamicProxy path:    {dpContextType,-40} ║");
        Console.WriteLine($"║  SourceGenerator path: {sgContextType,-40} ║");
        Console.WriteLine($"║  DP Proxy type:        {dpSvc.GetType().Name,-40} ║");
        Console.WriteLine($"║  SG Proxy type:        {sgSvc.GetType().Name,-40} ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Validate expectations
        if (dpContextType != "RuntimeAspectContext")
        {
            throw new InvalidOperationException(
                $"DynamicProxy path expected RuntimeAspectContext but got: {dpContextType}");
        }
        if (sgContextType != "SourceGeneratedAspectContext")
        {
            throw new InvalidOperationException(
                $"SourceGenerator path expected SourceGeneratedAspectContext but got: {sgContextType}");
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _vanillaProvider?.Dispose();
        _dynamicProxyProvider?.Dispose();
        _sourceGenProvider?.Dispose();
        _vanillaSingletonProvider?.Dispose();
        _dpSingletonProvider?.Dispose();
        _sgSingletonProvider?.Dispose();
    }

    // ── Transient resolution + invocation (measures full path) ───────────

    [Benchmark(Baseline = true, Description = "Vanilla MSDI: Resolve + Invoke")]
    public int Vanilla_ResolveAndInvoke()
    {
        var svc = _vanillaProvider.GetRequiredService<SampleMsdiServiceImpl>();
        return svc.Compute(10, 20);
    }

    [Benchmark(Description = "DynamicProxy MSDI: Resolve + Invoke")]
    public int DynamicProxy_ResolveAndInvoke()
    {
        var svc = _dynamicProxyProvider.GetRequiredService<SampleMsdiServiceImpl>();
        return svc.Compute(10, 20);
    }

    [Benchmark(Description = "SourceGen MSDI: Resolve + Invoke")]
    public int SourceGen_ResolveAndInvoke()
    {
        var svc = _sourceGenProvider.GetRequiredService<SampleMsdiServiceImpl>();
        return svc.Compute(10, 20);
    }

    // ── Hot path only: method invocation on pre-resolved proxy ───────────

    [Benchmark(Description = "Vanilla: Invoke only (pre-resolved)")]
    public int Vanilla_InvokeOnly()
    {
        return _vanillaSingleton.Compute(10, 20);
    }

    [Benchmark(Description = "DynamicProxy: Invoke only (pre-resolved)")]
    public int DynamicProxy_InvokeOnly()
    {
        return _dpSingleton.Compute(10, 20);
    }

    [Benchmark(Description = "SourceGen: Invoke only (pre-resolved)")]
    public int SourceGen_InvokeOnly()
    {
        return _sgSingleton.Compute(10, 20);
    }

    // ── Resolution-only (no method call) ─────────────────────────────────

    [Benchmark(Description = "Vanilla MSDI: Resolve only")]
    public object Vanilla_ResolveOnly()
    {
        return _vanillaProvider.GetRequiredService<SampleMsdiServiceImpl>();
    }

    [Benchmark(Description = "DynamicProxy MSDI: Resolve only")]
    public object DynamicProxy_ResolveOnly()
    {
        return _dynamicProxyProvider.GetRequiredService<SampleMsdiServiceImpl>();
    }

    [Benchmark(Description = "SourceGen MSDI: Resolve only")]
    public object SourceGen_ResolveOnly()
    {
        return _sourceGenProvider.GetRequiredService<SampleMsdiServiceImpl>();
    }
}

/// <summary>
/// Benchmarks scoped service resolution patterns through MSDI with AspectCore proxies.
/// Uses class-based registration to ensure SourceGenerator path exercises ClassProxy.
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("MSDI", "Scoped")]
public class MsdiScopedBenchmarks
{
    private ServiceProvider _vanillaProvider = null!;
    private ServiceProvider _dynamicProxyProvider = null!;
    private ServiceProvider _sourceGenProvider = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Vanilla
        var vanillaServices = new ServiceCollection();
        vanillaServices.AddScoped<SampleMsdiServiceImpl>();
        _vanillaProvider = vanillaServices.BuildServiceProvider();

        // DynamicProxy
        var dpServices = new ServiceCollection();
        dpServices.AddScoped<SampleMsdiServiceImpl>();
        dpServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        _dynamicProxyProvider = dpServices.BuildDynamicProxyProvider();

        // SourceGenerator
        var sgServices = new ServiceCollection();
        sgServices.AddScoped<SampleMsdiServiceImpl>();
        sgServices.ConfigureDynamicProxy(cfg =>
        {
            cfg.Interceptors.AddTyped<BenchmarkInterceptor>(Predicates.ForService("*MsdiService*"));
        });
        sgServices.ConfigureDynamicProxyEngine(opts =>
        {
            opts.Engine = ProxyEngine.SourceGenerator;
            opts.Strict = true;
            opts.AllowRuntimeFallback = false;
        });
        _sourceGenProvider = sgServices.BuildDynamicProxyProvider();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _vanillaProvider?.Dispose();
        _dynamicProxyProvider?.Dispose();
        _sourceGenProvider?.Dispose();
    }

    [Benchmark(Baseline = true, Description = "Vanilla MSDI: Scoped resolve + Invoke")]
    public int Vanilla_Scoped()
    {
        using var scope = _vanillaProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<SampleMsdiServiceImpl>();
        return svc.Compute(10, 20);
    }

    [Benchmark(Description = "DynamicProxy MSDI: Scoped resolve + Invoke")]
    public int DynamicProxy_Scoped()
    {
        using var scope = _dynamicProxyProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<SampleMsdiServiceImpl>();
        return svc.Compute(10, 20);
    }

    [Benchmark(Description = "SourceGen MSDI: Scoped resolve + Invoke")]
    public int SourceGen_Scoped()
    {
        using var scope = _sourceGenProvider.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<SampleMsdiServiceImpl>();
        return svc.Compute(10, 20);
    }
}

/// <summary>
/// Diagnostic interceptor that captures the runtime type name of the AspectContext.
/// Used during benchmark setup verification to confirm which context implementation
/// is being used (RuntimeAspectContext vs SourceGeneratedAspectContext).
/// </summary>
public sealed class ContextTypeCapturingInterceptor : AbstractInterceptorAttribute
{
    [ThreadStatic]
    public static string? CapturedTypeName;

    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        CapturedTypeName = context.GetType().Name;
        return next(context);
    }
}
