using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using BenchmarkDotNet.Attributes;

namespace AspectCore.Benchmarks;

/// <summary>
/// Base class that sets up proxy generators for both engines.
/// </summary>
public abstract class ProxyBenchmarkBase
{
    protected IProxyGenerator DynamicProxyGen = null!;
    protected IProxyGenerator SourceGenProxyGen = null!;

    [GlobalSetup]
    public virtual void Setup()
    {
        DynamicProxyGen = CreateGenerator(ProxyEngine.DynamicProxy);
        SourceGenProxyGen = CreateGenerator(ProxyEngine.SourceGenerator);
    }

    private static IProxyGenerator CreateGenerator(ProxyEngine engine)
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

// ── Sync method benchmarks ───────────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Sync")]
public class SyncMethodBenchmarks : ProxyBenchmarkBase
{
    private SyncService _dynamicProxy = null!;
    private SyncService _sourceGenProxy = null!;
    private SyncService _direct = null!;

    public override void Setup()
    {
        base.Setup();
        _direct = new SyncService();
        _dynamicProxy = (SyncService)DynamicProxyGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
        _sourceGenProxy = (SyncService)SourceGenProxyGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct call (no proxy)")]
    public int Direct() => _direct.Add(1, 2);

    [Benchmark(Description = "DynamicProxy: Add(1,2)")]
    public int DynamicProxy_Add() => _dynamicProxy.Add(1, 2);

    [Benchmark(Description = "SourceGenerator: Add(1,2)")]
    public int SourceGen_Add() => _sourceGenProxy.Add(1, 2);

    [Benchmark(Description = "DynamicProxy: Concat(3 strings)")]
    public string DynamicProxy_Concat() => _dynamicProxy.Concat("a", "b", "c");

    [Benchmark(Description = "SourceGenerator: Concat(3 strings)")]
    public string SourceGen_Concat() => _sourceGenProxy.Concat("a", "b", "c");

    [Benchmark(Description = "DynamicProxy: ManyParams(8)")]
    public int DynamicProxy_ManyParams() => _dynamicProxy.ManyParams(1, 2, 3, 4, 5, 6, 7, 8);

    [Benchmark(Description = "SourceGenerator: ManyParams(8)")]
    public int SourceGen_ManyParams() => _sourceGenProxy.ManyParams(1, 2, 3, 4, 5, 6, 7, 8);
}

// ── Async method benchmarks ─────────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Async")]
public class AsyncMethodBenchmarks : ProxyBenchmarkBase
{
    private AsyncService _dynamicProxy = null!;
    private AsyncService _sourceGenProxy = null!;
    private AsyncService _direct = null!;

    public override void Setup()
    {
        base.Setup();
        _direct = new AsyncService();
        _dynamicProxy = (AsyncService)DynamicProxyGen.CreateClassProxy(typeof(AsyncService), typeof(AsyncService), Array.Empty<object>());
        _sourceGenProxy = (AsyncService)SourceGenProxyGen.CreateClassProxy(typeof(AsyncService), typeof(AsyncService), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: Task AddAsync")]
    public Task<int> Direct_Task() => _direct.AddAsync(1, 2);

    [Benchmark(Description = "DynamicProxy: Task AddAsync")]
    public Task<int> DynamicProxy_Task() => _dynamicProxy.AddAsync(1, 2);

    [Benchmark(Description = "SourceGenerator: Task AddAsync")]
    public Task<int> SourceGen_Task() => _sourceGenProxy.AddAsync(1, 2);

    [Benchmark(Description = "Direct: ValueTask AddAsync")]
    public ValueTask<int> Direct_ValueTask() => _direct.AddValueTaskAsync(1, 2);

    [Benchmark(Description = "DynamicProxy: ValueTask AddAsync")]
    public ValueTask<int> DynamicProxy_ValueTask() => _dynamicProxy.AddValueTaskAsync(1, 2);

    [Benchmark(Description = "SourceGenerator: ValueTask AddAsync")]
    public ValueTask<int> SourceGen_ValueTask() => _sourceGenProxy.AddValueTaskAsync(1, 2);
}

// ── Property benchmarks ─────────────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Property")]
public class PropertyBenchmarks : ProxyBenchmarkBase
{
    private PropertyService _dynamicProxy = null!;
    private PropertyService _sourceGenProxy = null!;
    private PropertyService _direct = null!;

    public override void Setup()
    {
        base.Setup();
        _direct = new PropertyService();
        _dynamicProxy = (PropertyService)DynamicProxyGen.CreateClassProxy(typeof(PropertyService), typeof(PropertyService), Array.Empty<object>());
        _sourceGenProxy = (PropertyService)SourceGenProxyGen.CreateClassProxy(typeof(PropertyService), typeof(PropertyService), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: get_Value")]
    public int Direct_Get() => _direct.Value;

    [Benchmark(Description = "DynamicProxy: get_Value")]
    public int DynamicProxy_Get() => _dynamicProxy.Value;

    [Benchmark(Description = "SourceGenerator: get_Value")]
    public int SourceGen_Get() => _sourceGenProxy.Value;

    [Benchmark(Description = "Direct: set_Value")]
    public void Direct_Set() => _direct.Value = 42;

    [Benchmark(Description = "DynamicProxy: set_Value")]
    public void DynamicProxy_Set() => _dynamicProxy.Value = 42;

    [Benchmark(Description = "SourceGenerator: set_Value")]
    public void SourceGen_Set() => _sourceGenProxy.Value = 42;
}

// ── Generic method benchmarks ───────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Generic")]
public class GenericMethodBenchmarks : ProxyBenchmarkBase
{
    private GenericService<int> _dynamicProxy = null!;
    private GenericService<int> _sourceGenProxy = null!;
    private GenericService<int> _direct = null!;

    public override void Setup()
    {
        base.Setup();
        _direct = new GenericService<int>();
        _dynamicProxy = (GenericService<int>)DynamicProxyGen.CreateClassProxy(typeof(GenericService<int>), typeof(GenericService<int>), Array.Empty<object>());
        _sourceGenProxy = (GenericService<int>)SourceGenProxyGen.CreateClassProxy(typeof(GenericService<int>), typeof(GenericService<int>), Array.Empty<object>());
    }

    [Benchmark(Baseline = true, Description = "Direct: Echo<int>")]
    public int Direct_Echo() => _direct.Echo(42);

    [Benchmark(Description = "DynamicProxy: Echo<int>")]
    public int DynamicProxy_Echo() => _dynamicProxy.Echo(42);

    [Benchmark(Description = "SourceGenerator: Echo<int>")]
    public int SourceGen_Echo() => _sourceGenProxy.Echo(42);
}

// ── Interface proxy benchmarks ─────────────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Interface")]
public class InterfaceProxyBenchmarks : ProxyBenchmarkBase
{
    private IInterfaceService _dynamicProxy = null!;
    private IInterfaceService _sourceGenProxy = null!;
    private IInterfaceService _direct = null!;

    public override void Setup()
    {
        base.Setup();
        _direct = new InterfaceServiceImpl();
        _dynamicProxy = (IInterfaceService)DynamicProxyGen.CreateInterfaceProxy(typeof(IInterfaceService), _direct);
        _sourceGenProxy = (IInterfaceService)SourceGenProxyGen.CreateInterfaceProxy(typeof(IInterfaceService), _direct);
    }

    [Benchmark(Baseline = true, Description = "Direct: IInterfaceService.Add")]
    public int Direct_Add() => _direct.Add(1, 2);

    [Benchmark(Description = "DynamicProxy: IInterfaceService.Add")]
    public int DynamicProxy_Add() => _dynamicProxy.Add(1, 2);

    [Benchmark(Description = "SourceGenerator: IInterfaceService.Add")]
    public int SourceGen_Add() => _sourceGenProxy.Add(1, 2);
}

// ── Proxy creation overhead benchmarks ─────────────────────────────────

[MemoryDiagnoser]
[BenchmarkCategory("Creation")]
public class ProxyCreationBenchmarks : ProxyBenchmarkBase
{
    [Benchmark(Baseline = true, Description = "DynamicProxy: CreateClassProxy")]
    public object DynamicProxy_Create()
    {
        return DynamicProxyGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
    }

    [Benchmark(Description = "SourceGenerator: CreateClassProxy")]
    public object SourceGen_Create()
    {
        return SourceGenProxyGen.CreateClassProxy(typeof(SyncService), typeof(SyncService), Array.Empty<object>());
    }

    [Benchmark(Description = "DynamicProxy: CreateInterfaceProxy")]
    public object DynamicProxy_CreateInterface()
    {
        return DynamicProxyGen.CreateInterfaceProxy(typeof(IInterfaceService), new InterfaceServiceImpl());
    }

    [Benchmark(Description = "SourceGenerator: CreateInterfaceProxy")]
    public object SourceGen_CreateInterface()
    {
        return SourceGenProxyGen.CreateInterfaceProxy(typeof(IInterfaceService), new InterfaceServiceImpl());
    }
}
