using System.Runtime.CompilerServices;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Benchmarks;

// ── RefOut parameter benchmarks ─────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("RefOut")]
public class RefOutBenchmarks : ProxyBenchmarkBase
{
    private RefOutService _direct = null!;
    private RefOutService _dynamicProxy = null!;
    private RefOutService _sourceGenProxy = null!;

    public override void Setup()
    {
        base.Setup();
        _direct = new RefOutService();
        _dynamicProxy = (RefOutService)DynamicProxyGen.CreateClassProxy(typeof(RefOutService), typeof(RefOutService), Array.Empty<object>());
        _sourceGenProxy = (RefOutService)SourceGenProxyGen.CreateClassProxy(typeof(RefOutService), typeof(RefOutService), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: TryParse(out)")]
    public bool Direct_TryParse()
    {
        return _direct.TryParse("42", out _);
    }

    [Benchmark(Description = "DynamicProxy: TryParse(out)")]
    public bool DynamicProxy_TryParse()
    {
        return _dynamicProxy.TryParse("42", out _);
    }

    [Benchmark(Description = "SourceGenerator: TryParse(out)")]
    public bool SourceGen_TryParse()
    {
        return _sourceGenProxy.TryParse("42", out _);
    }

    [Benchmark(Description = "Direct: Swap(ref, ref)")]
    public void Direct_Swap()
    {
        int a = 1, b = 2;
        _direct.Swap(ref a, ref b);
    }

    [Benchmark(Description = "DynamicProxy: Swap(ref, ref)")]
    public void DynamicProxy_Swap()
    {
        int a = 1, b = 2;
        _dynamicProxy.Swap(ref a, ref b);
    }

    [Benchmark(Description = "SourceGenerator: Swap(ref, ref)")]
    public void SourceGen_Swap()
    {
        int a = 1, b = 2;
        _sourceGenProxy.Swap(ref a, ref b);
    }
}

// ── Ref return benchmarks ───────────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("RefReturn")]
public class RefReturnBenchmarks : ProxyBenchmarkBase
{
    private RefReturnService _direct = null!;
    private RefReturnService _dynamicProxy = null!;
    private RefReturnService _sourceGenProxy = null!;

    public override void Setup()
    {
        base.Setup();
        _direct = new RefReturnService();
        _dynamicProxy = (RefReturnService)DynamicProxyGen.CreateClassProxy(typeof(RefReturnService), typeof(RefReturnService), Array.Empty<object>());
        _sourceGenProxy = (RefReturnService)SourceGenProxyGen.CreateClassProxy(typeof(RefReturnService), typeof(RefReturnService), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: ref int GetRef()")]
    public ref int Direct_GetRef() => ref _direct.GetRef();

    [Benchmark(Description = "DynamicProxy: ref int GetRef()")]
    public ref int DynamicProxy_GetRef() => ref _dynamicProxy.GetRef();

    [Benchmark(Description = "SourceGenerator: ref int GetRef()")]
    public ref int SourceGen_GetRef() => ref _sourceGenProxy.GetRef();
}

// ── IAsyncEnumerable benchmarks ─────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("AsyncEnumerable")]
public class AsyncEnumerableBenchmarks : ProxyBenchmarkBase
{
    private AsyncEnumerableService _direct = null!;
    private AsyncEnumerableService _dynamicProxy = null!;
    private AsyncEnumerableService _sourceGenProxy = null!;

    [Params(10, 100)]
    public int ItemCount { get; set; }

    public override void Setup()
    {
        base.Setup();
        _direct = new AsyncEnumerableService();
        _dynamicProxy = (AsyncEnumerableService)DynamicProxyGen.CreateClassProxy(typeof(AsyncEnumerableService), typeof(AsyncEnumerableService), Array.Empty<object>());
        _sourceGenProxy = (AsyncEnumerableService)SourceGenProxyGen.CreateClassProxy(typeof(AsyncEnumerableService), typeof(AsyncEnumerableService), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: IAsyncEnumerable<int>")]
    public async Task<int> Direct_GetNumbers()
    {
        int sum = 0;
        await foreach (var item in _direct.GetNumbers(ItemCount))
        {
            sum += item;
        }
        return sum;
    }

    [Benchmark(Description = "DynamicProxy: IAsyncEnumerable<int>")]
    public async Task<int> DynamicProxy_GetNumbers()
    {
        int sum = 0;
        await foreach (var item in _dynamicProxy.GetNumbers(ItemCount))
        {
            sum += item;
        }
        return sum;
    }

    [Benchmark(Description = "SourceGenerator: IAsyncEnumerable<int>")]
    public async Task<int> SourceGen_GetNumbers()
    {
        int sum = 0;
        await foreach (var item in _sourceGenProxy.GetNumbers(ItemCount))
        {
            sum += item;
        }
        return sum;
    }
}

// ── Exception path benchmarks ───────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Exception")]
public class ExceptionBenchmarks
{
    private SyncService _dpThrowing = null!;
    private SyncService _sgThrowing = null!;

    [GlobalSetup]
    public void Setup()
    {
        _dpThrowing = CreateThrowingProxy(ProxyEngine.DynamicProxy);
        _sgThrowing = CreateThrowingProxy(ProxyEngine.SourceGenerator);
    }

    private static SyncService CreateThrowingProxy(ProxyEngine engine)
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
            cfg.Interceptors.AddTyped<ThrowingInterceptor>(Predicates.ForService("*Service*"));
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

    [Benchmark(Baseline = true, Description = "DynamicProxy: Exception path")]
    public bool DynamicProxy_Exception()
    {
        try
        {
            _dpThrowing.Add(1, 2);
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }

    [Benchmark(Description = "SourceGenerator: Exception path")]
    public bool SourceGen_Exception()
    {
        try
        {
            _sgThrowing.Add(1, 2);
            return false;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }
}

// ── Concurrency benchmarks ──────────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Concurrency")]
public class ConcurrencyBenchmarks : ProxyBenchmarkBase
{
    private SyncService _dynamicProxy = null!;
    private SyncService _sourceGenProxy = null!;

    [Params(1, 4, 8)]
    public int ThreadCount { get; set; }

    public override void Setup()
    {
        base.Setup();
        _dynamicProxy = (SyncService)DynamicProxyGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
        _sourceGenProxy = (SyncService)SourceGenProxyGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "DynamicProxy: Concurrent calls")]
    public Task DynamicProxy_Concurrent()
    {
        var tasks = new Task[ThreadCount];
        for (int i = 0; i < ThreadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    _dynamicProxy.Add(j, j);
                }
            });
        }
        return Task.WhenAll(tasks);
    }

    [Benchmark(Description = "SourceGenerator: Concurrent calls")]
    public Task SourceGen_Concurrent()
    {
        var tasks = new Task[ThreadCount];
        for (int i = 0; i < ThreadCount; i++)
        {
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < 100; j++)
                {
                    _sourceGenProxy.Add(j, j);
                }
            });
        }
        return Task.WhenAll(tasks);
    }
}

// ── Large struct benchmarks ─────────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("LargeStruct")]
public class LargeStructBenchmarks : ProxyBenchmarkBase
{
    private LargeStructService _direct = null!;
    private LargeStructService _dynamicProxy = null!;
    private LargeStructService _sourceGenProxy = null!;
    private LargeStruct _data;

    public override void Setup()
    {
        base.Setup();
        _direct = new LargeStructService();
        _dynamicProxy = (LargeStructService)DynamicProxyGen.CreateClassProxy(typeof(LargeStructService), typeof(LargeStructService), Array.Empty<object>());
        _sourceGenProxy = (LargeStructService)SourceGenProxyGen.CreateClassProxy(typeof(LargeStructService), typeof(LargeStructService), Array.Empty<object>());
        _data = new LargeStruct
        {
            Field1 = 1, Field2 = 2, Field3 = 3, Field4 = 4,
            Field5 = 5, Field6 = 6, Field7 = 7, Field8 = 8
        };
    }

    [Benchmark(Baseline = true, Description = "Direct: Process(LargeStruct)")]
    public long Direct_Process() => _direct.Process(_data);

    [Benchmark(Description = "DynamicProxy: Process(LargeStruct)")]
    public long DynamicProxy_Process() => _dynamicProxy.Process(_data);

    [Benchmark(Description = "SourceGenerator: Process(LargeStruct)")]
    public long SourceGen_Process() => _sourceGenProxy.Process(_data);
}
