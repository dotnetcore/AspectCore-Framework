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

// ── A simple interceptor for benchmarking ────────────────────────────────

public sealed class BenchmarkInterceptor : AbstractInterceptorAttribute
{
    public override Task Invoke(AspectContext context, AspectDelegate next)
    {
        return next(context);
    }
}
