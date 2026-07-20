using AspectCore.DynamicProxy;

namespace AspectCore.Benchmarks.Competitive;

// ── Shared service contracts for benchmarking ──────────────────────────────

/// <summary>
/// Simple interface for proxy generation across all frameworks.
/// </summary>
public interface ICalculator
{
    int Add(int a, int b);
    string Concat(string a, string b);
}

/// <summary>
/// Async service interface.
/// </summary>
public interface IAsyncCalculator
{
    Task<int> AddAsync(int a, int b);
    ValueTask<int> MultiplyAsync(int a, int b);
}

/// <summary>
/// Implementation used by Castle and AspectCore interface proxies.
/// </summary>
public class CalculatorImpl : ICalculator
{
    public int Add(int a, int b) => a + b;
    public string Concat(string a, string b) => a + b;
}

/// <summary>
/// Async implementation.
/// </summary>
public class AsyncCalculatorImpl : IAsyncCalculator
{
    public Task<int> AddAsync(int a, int b) => Task.FromResult(a + b);
    public ValueTask<int> MultiplyAsync(int a, int b) => new(a * b);
}

/// <summary>
/// Class-based proxy target for AspectCore (virtual methods).
/// </summary>
[AspectCoreGenerateProxy]
public class VirtualCalculator
{
    public virtual int Add(int a, int b) => a + b;
    public virtual string Concat(string a, string b) => a + b;
}

/// <summary>
/// Async class-based proxy target for AspectCore.
/// </summary>
[AspectCoreGenerateProxy]
public class VirtualAsyncCalculator
{
    public virtual Task<int> AddAsync(int a, int b) => Task.FromResult(a + b);
    public virtual ValueTask<int> MultiplyAsync(int a, int b) => new(a * b);
}
