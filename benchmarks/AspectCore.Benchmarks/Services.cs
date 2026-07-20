using AspectCore.DynamicProxy;

namespace AspectCore.Benchmarks;

// ── Services used in benchmarks ──────────────────────────────────────────

[AspectCoreGenerateProxy]
public class SyncService
{
    public virtual int Add(int a, int b) => a + b;

    public virtual string Concat(string a, string b, string c) => a + b + c;

    public virtual int ManyParams(int a, int b, int c, int d, int e, int f, int g, int h)
        => a + b + c + d + e + f + g + h;
}

[AspectCoreGenerateProxy]
public class AsyncService
{
    public virtual async Task<int> AddAsync(int a, int b)
    {
        await Task.Yield();
        return a + b;
    }

    public virtual async ValueTask<int> AddValueTaskAsync(int a, int b)
    {
        await Task.Yield();
        return a + b;
    }
}

[AspectCoreGenerateProxy]
public class PropertyService
{
    public virtual int Value { get; set; }

    public virtual string Name { get; set; } = string.Empty;
}

[AspectCoreGenerateProxy]
public class GenericService<T>
{
    public virtual T Echo(T value) => value;

    public virtual List<T> Wrap(T value) => new() { value };
}

public interface IInterfaceService
{
    int Add(int a, int b);
    string Concat(string a, string b);
}

[AspectCoreGenerateProxy]
public class InterfaceServiceImpl : IInterfaceService
{
    public virtual int Add(int a, int b) => a + b;
    public virtual string Concat(string a, string b) => a + b;
}

// ── MSDI integration service contracts ───────────────────────────────────

public interface ISampleMsdiService
{
    int Compute(int a, int b);
    string Format(string input);
}

[AspectCoreGenerateProxy]
public class SampleMsdiServiceImpl : ISampleMsdiService
{
    public virtual int Compute(int a, int b) => a + b;
    public virtual string Format(string input) => $"[{input}]";
}

// ── Additional services for advanced benchmarks ─────────────────────────

[AspectCoreGenerateProxy]
public class RefOutService
{
    public virtual bool TryParse(string input, out int result)
    {
        return int.TryParse(input, out result);
    }

    public virtual void Swap(ref int a, ref int b)
    {
        (a, b) = (b, a);
    }
}

[AspectCoreGenerateProxy]
public class RefReturnService
{
    private int _value = 42;

    public virtual ref int GetRef() => ref _value;
}

[AspectCoreGenerateProxy]
public class AsyncEnumerableService
{
    public virtual async IAsyncEnumerable<int> GetNumbers(int count)
    {
        for (int i = 0; i < count; i++)
        {
            await Task.Yield();
            yield return i;
        }
    }
}

[AspectCoreGenerateProxy]
public class LargeStructService
{
    public virtual long Process(LargeStruct data) => data.Field1 + data.Field8;
}

/// <summary>
/// A large value type (64 bytes) for measuring boxing/copying overhead.
/// </summary>
public struct LargeStruct
{
    public long Field1, Field2, Field3, Field4, Field5, Field6, Field7, Field8;
}

// ── A simple interceptor for benchmarking ────────────────────────────────

public sealed class BenchmarkInterceptor : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        return next(context);
    }
}

/// <summary>
/// An interceptor that always throws, for benchmarking exception paths.
/// </summary>
public sealed class ThrowingInterceptor : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        throw new InvalidOperationException("Benchmark exception");
    }
}
