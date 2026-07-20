using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.NativeAot.E2E;
using Microsoft.Extensions.DependencyInjection;

// ============================================================================
// NativeAOT E2E Test Runner
// Exercises real interception scenarios using source-generated proxies.
// Exit code 0 = all pass, non-zero = failure count.
// ============================================================================

var results = new List<(string Name, bool Passed, string? Error)>();

void Assert(bool condition, string message)
{
    if (!condition) throw new Exception(message);
}

void RunTest(string name, Action test)
{
    try
    {
        LogInterceptorAttribute.Invocations.Clear();
        SecondInterceptorAttribute.Invocations.Clear();
        test();
        results.Add((name, true, null));
        Console.WriteLine($"  PASS: {name}");
    }
    catch (Exception ex)
    {
        var msg = ex.InnerException != null
            ? $"{ex.Message} -> {ex.InnerException.Message}\n{ex.InnerException.StackTrace}"
            : $"{ex.Message}\n{ex.StackTrace}";
        results.Add((name, false, ex.Message));
        Console.WriteLine($"  FAIL: {name} - {msg}");
    }
}

async Task RunTestAsync(string name, Func<Task> test)
{
    try
    {
        LogInterceptorAttribute.Invocations.Clear();
        SecondInterceptorAttribute.Invocations.Clear();
        await test();
        results.Add((name, true, null));
        Console.WriteLine($"  PASS: {name}");
    }
    catch (Exception ex)
    {
        results.Add((name, false, ex.Message));
        Console.WriteLine($"  FAIL: {name} - {ex.Message}");
    }
}

// Build the DI container with source-generated proxy engine
var services = new ServiceCollection();
services.AddTransient<IBasicService, BasicService>();
services.AddTransient<ClassService>();
services.AddKeyedTransient<IKeyedService, KeyedServiceA>("A");
services.AddKeyedTransient<IKeyedService, KeyedServiceB>("B");
services.ConfigureDynamicProxy();
services.ConfigureDynamicProxyEngine(options =>
{
    options.Engine = ProxyEngine.SourceGenerator;
    options.Strict = true;
});
// Manually register the source-generated proxy registry for NativeAOT safety.
// Assembly scanning may not work reliably when trimming is enabled.
// Use factory registration to avoid DI constructor resolution issues in AOT.
services.AddSingleton<AspectCore.DynamicProxy.ISourceGeneratedProxyRegistry>(
    _ => new AspectCore.SourceGenerated.AspectCoreSourceGeneratedProxyRegistry());

var provider = services.BuildDynamicProxyProvider();

Console.WriteLine("=== AspectCore NativeAOT E2E Tests ===");
Console.WriteLine();

// Resolve services
var basicService = provider.GetRequiredService<IBasicService>();
var classService = provider.GetRequiredService<ClassService>();

// --- Scenario 1: Sync method with return value ---
RunTest("Sync method with return value", () =>
{
    var result = basicService.Add(3, 4);
    Assert(result == 7, $"Expected 7, got {result}");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:Add"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:Add"), "Interceptor After not called");
});

// --- Scenario 2: Sync void method ---
RunTest("Sync void method", () =>
{
    basicService.DoNothing();
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:DoNothing"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:DoNothing"), "Interceptor After not called");
});

// --- Scenario 3: Async Task<T> method ---
await RunTestAsync("Async Task<T> method", async () =>
{
    var result = await basicService.MultiplyAsync(3, 4);
    Assert(result == 12, $"Expected 12, got {result}");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:MultiplyAsync"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:MultiplyAsync"), "Interceptor After not called");
});

// --- Scenario 4: Async Task (non-generic) method ---
await RunTestAsync("Async Task (non-generic) method", async () =>
{
    await basicService.DoNothingAsync();
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:DoNothingAsync"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:DoNothingAsync"), "Interceptor After not called");
});

// --- Scenario 5: Async ValueTask<T> method ---
await RunTestAsync("Async ValueTask<T> method", async () =>
{
    var result = await basicService.DivideAsync(10, 2);
    Assert(result == 5, $"Expected 5, got {result}");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:DivideAsync"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:DivideAsync"), "Interceptor After not called");
});

// --- Scenario 6: Async ValueTask (non-generic) method ---
await RunTestAsync("Async ValueTask (non-generic) method", async () =>
{
    await basicService.DoNothingValueTaskAsync();
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:DoNothingValueTaskAsync"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:DoNothingValueTaskAsync"), "Interceptor After not called");
});

// --- Scenario 7: IAsyncEnumerable<T> method ---
await RunTestAsync("IAsyncEnumerable<T> method", async () =>
{
    var items = new List<int>();
    await foreach (var item in basicService.GetNumbersAsync(3))
    {
        items.Add(item);
    }
    Assert(items.Count == 3, $"Expected 3 items, got {items.Count}");
    Assert(items.SequenceEqual(new[] { 1, 2, 3 }), $"Expected [1,2,3], got [{string.Join(",", items)}]");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:GetNumbersAsync"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:GetNumbersAsync"), "Interceptor After not called");
});

// --- Scenario 8: Method with ref/out parameters ---
RunTest("Method with out parameter", () =>
{
    basicService.GetOutput(5, out int doubled);
    Assert(doubled == 10, $"Expected 10, got {doubled}");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:GetOutput"), "Interceptor Before not called");
});

RunTest("Method with ref parameter", () =>
{
    int value = 5;
    basicService.Increment(ref value);
    Assert(value == 6, $"Expected 6, got {value}");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:Increment"), "Interceptor Before not called");
});

// --- Scenario 9: Multiple interceptors stacked ---
RunTest("Multiple interceptors stacked", () =>
{
    var result = basicService.Stacked("test");
    Assert(result == "result:test", $"Expected 'result:test', got '{result}'");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:Stacked"), "LogInterceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:Stacked"), "LogInterceptor After not called");
    Assert(SecondInterceptorAttribute.Invocations.Contains("Second:Before:Stacked"), "SecondInterceptor Before not called");
    Assert(SecondInterceptorAttribute.Invocations.Contains("Second:After:Stacked"), "SecondInterceptor After not called");
});

// --- Scenario 10: Interceptor modifying parameters ---
RunTest("Interceptor modifying parameters", () =>
{
    var result = basicService.ParamModified(5);
    // ParamModifierInterceptor doubles the first int param: 5 -> 10
    // ParamModified returns value as-is, so result = 10
    Assert(result == 10, $"Expected 10, got {result}");
});

// --- Scenario 11: Interceptor modifying return value ---
RunTest("Interceptor modifying return value", () =>
{
    var result = basicService.ReturnModified(42);
    // ReturnModifierInterceptor adds 100 to int return: 42 -> 142
    Assert(result == 142, $"Expected 142, got {result}");
});

// --- Scenario 12: Keyed service interception ---
RunTest("Keyed service interception", () =>
{
    var serviceA = provider.GetRequiredKeyedService<IKeyedService>("A");
    var serviceB = provider.GetRequiredKeyedService<IKeyedService>("B");
    var keyA = serviceA.GetKey();
    var keyB = serviceB.GetKey();
    Assert(keyA == "ServiceA", $"Expected 'ServiceA', got '{keyA}'");
    Assert(keyB == "ServiceB", $"Expected 'ServiceB', got '{keyB}'");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:GetKey"), "Interceptor Before not called");
});

// --- Scenario 13: Interface proxy (most common) ---
RunTest("Interface proxy", () =>
{
    // basicService is already an interface proxy - verify it's a proxy
    var proxyType = basicService.GetType();
    Assert(proxyType.Name != nameof(BasicService), $"Expected proxy type, got {proxyType.Name}");
    Assert(proxyType.GetCustomAttributes(typeof(DynamicallyAttribute), false).Length > 0,
        "Expected [Dynamically] attribute on proxy type");
});

// --- Scenario 14: Class proxy ---
RunTest("Class proxy", () =>
{
    var result = classService.Calculate(5);
    Assert(result == 15, $"Expected 15, got {result}");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:Calculate"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:Calculate"), "Interceptor After not called");

    LogInterceptorAttribute.Invocations.Clear();
    var greeting = classService.Greet("World");
    Assert(greeting == "Hello, World", $"Expected 'Hello, World', got '{greeting}'");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:Greet"), "Interceptor Before not called");
});

// --- Scenario 15: Generic method (falls back to reflection in SG, still works) ---
RunTest("Generic method interception", () =>
{
    var result = basicService.Echo(42);
    Assert(result == 42, $"Expected 42, got {result}");
    Assert(LogInterceptorAttribute.Invocations.Contains("Before:Echo"), "Interceptor Before not called");
    Assert(LogInterceptorAttribute.Invocations.Contains("After:Echo"), "Interceptor After not called");

    LogInterceptorAttribute.Invocations.Clear();
    var strResult = basicService.Echo("hello");
    Assert(strResult == "hello", $"Expected 'hello', got '{strResult}'");
});

// ============================================================================
// Summary
// ============================================================================
Console.WriteLine();
Console.WriteLine("=== Results ===");
var passed = results.Count(r => r.Passed);
var failed = results.Count(r => !r.Passed);
Console.WriteLine($"Total: {results.Count}, Passed: {passed}, Failed: {failed}");

if (failed > 0)
{
    Console.WriteLine();
    Console.WriteLine("Failed tests:");
    foreach (var (name, _, error) in results.Where(r => !r.Passed))
    {
        Console.WriteLine($"  - {name}: {error}");
    }
}

Console.WriteLine();
Console.WriteLine(failed == 0 ? "ALL TESTS PASSED" : "SOME TESTS FAILED");
return failed;
