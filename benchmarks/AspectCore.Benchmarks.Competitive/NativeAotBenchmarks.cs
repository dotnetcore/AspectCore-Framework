using BenchmarkDotNet.Attributes;

namespace AspectCore.Benchmarks.Competitive;

/// <summary>
/// Documents NativeAOT compatibility differences between the frameworks.
/// 
/// Castle DynamicProxy relies on System.Reflection.Emit which is NOT available
/// in NativeAOT. This means Castle proxies cannot be created at runtime in 
/// NativeAOT-published applications.
/// 
/// AspectCore supports NativeAOT via its Source Generator engine, which generates
/// proxy types at compile time. No runtime IL emission is required.
/// 
/// This benchmark class serves as documentation and runs in JIT mode to show
/// the performance characteristics. In actual NativeAOT deployment:
/// - Castle: FAILS at runtime (TypeLoadException / PlatformNotSupportedException)
/// - AspectCore-DP: FAILS at runtime (same reason - uses Reflection.Emit)
/// - AspectCore-SG: WORKS (compile-time generated proxies)
/// </summary>
[MemoryDiagnoser]
[BenchmarkCategory("NativeAOT")]
public class NativeAotCompatibilityBenchmarks
{
    /*
     * ┌─────────────────────────────────┬─────────────┬──────────────────┐
     * │ Framework                       │ JIT Mode    │ NativeAOT Mode   │
     * ├─────────────────────────────────┼─────────────┼──────────────────┤
     * │ Castle DynamicProxy             │ Works       │ FAILS            │
     * │ AspectCore DynamicProxy Engine  │ Works       │ FAILS            │
     * │ AspectCore SourceGenerator      │ Works       │ Works            │
     * └─────────────────────────────────┴─────────────┴──────────────────┘
     * 
     * Key insight: Only AspectCore's Source Generator engine provides
     * NativeAOT-compatible AOP. Castle has announced plans for a source
     * generator in v6, but as of Castle.Core 5.2.1 there is no release date.
     */

    private VirtualCalculator _sgProxy = null!;

    [GlobalSetup]
    public void Setup()
    {
        // Only AspectCore-SG would work in NativeAOT
        var sgGen = BenchmarkHelpers.CreateAspectCoreGenerator(
            AspectCore.DynamicProxy.ProxyEngine.SourceGenerator);
        _sgProxy = (VirtualCalculator)sgGen.CreateClassProxy(
            typeof(VirtualCalculator), typeof(VirtualCalculator), Array.Empty<object>());
    }

    [Benchmark(Description = "AspectCore-SG: NativeAOT-compatible call")]
    public int AspectCore_SG_NativeAotCompatible() => _sgProxy.Add(1, 2);

    [Benchmark(Baseline = true, Description = "Direct: baseline")]
    public int Direct() => 1 + 2;
}
