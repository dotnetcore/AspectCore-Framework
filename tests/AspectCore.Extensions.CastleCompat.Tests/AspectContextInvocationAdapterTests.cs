using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.CastleCompat;
using Castle.DynamicProxy;
using Xunit;
using CastleProxyGenerator = Castle.DynamicProxy.ProxyGenerator;

namespace AspectCore.Extensions.CastleCompat.Tests;

// ── Additional service types for adapter tests ─────────────────────────

public interface IGenericService
{
    T Echo<T>(T value);
    string NonGeneric(string input);
}

public class GenericServiceImpl : IGenericService
{
    public T Echo<T>(T value) => value;
    public string NonGeneric(string input) => input;
}

public interface IVoidService
{
    void DoWork(string input);
}

public class VoidServiceImpl : IVoidService
{
    public string LastInput { get; private set; } = "";
    public void DoWork(string input) => LastInput = input;
}

/// <summary>
/// Tests for AspectContextInvocationAdapter (internal class).
/// The adapter is created inside CastleInterceptorAdapter.Invoke(), so we test it
/// by calling Invoke() with a capturing Castle interceptor, which receives our adapter.
/// </summary>
public class AspectContextInvocationAdapterTests
{
    // ── Helpers ─────────────────────────────────────────────────────────

    private static readonly Type AdapterType =
        typeof(CastleInterceptorAdapter).Assembly
            .GetType("AspectCore.Extensions.CastleCompat.AspectContextInvocationAdapter")!;

    private static readonly Type InvocationAspectContextType =
        typeof(CastleInterceptorAdapter).Assembly
            .GetType("AspectCore.Extensions.CastleCompat.InvocationAspectContext")!;

    /// <summary>
    /// Creates an InvocationAspectContext backed by a real Castle IInvocation.
    /// The IInvocation is obtained from a fully-proceeded proxy call.
    /// </summary>
    private static AspectContext CreateRealAspectContext(out IInvocation backingInvocation)
    {
        IInvocation captured = null!;
        var interceptor = new InlineInterceptor(inv =>
        {
            captured = inv;
            inv.Proceed();
        });
        var generator = new CastleProxyGenerator();
        var proxy = generator.CreateInterfaceProxyWithTarget<IGreetService>(new GreetService(), interceptor);
        proxy.Add(10, 20);

        backingInvocation = captured;
        var ctor = InvocationAspectContextType.GetConstructor(new[] { typeof(IInvocation) })!;
        return (AspectContext)ctor.Invoke(new object[] { captured });
    }

    private static AspectContext CreateGenericAspectContext(bool useGenericMethod)
    {
        IInvocation captured = null!;
        var interceptor = new InlineInterceptor(inv =>
        {
            captured = inv;
            inv.Proceed();
        });
        var generator = new CastleProxyGenerator();
        var proxy = generator.CreateInterfaceProxyWithTarget<IGenericService>(new GenericServiceImpl(), interceptor);

        if (useGenericMethod)
            proxy.Echo("hello");
        else
            proxy.NonGeneric("hello");

        var ctor = InvocationAspectContextType.GetConstructor(new[] { typeof(IInvocation) })!;
        return (AspectContext)ctor.Invoke(new object[] { captured });
    }

    /// <summary>
    /// Invokes CastleInterceptorAdapter.Invoke() with a capturing Castle interceptor.
    /// Returns the IInvocation (AspectContextInvocationAdapter) received by the Castle interceptor.
    /// </summary>
    private static IInvocation InvokeAdapterAndCapture(
        AspectContext context,
        AspectDelegate next,
        Action<IInvocation> extraAction = null)
    {
        IInvocation captured = null!;
        var castleInterceptor = new InlineInterceptor(inv =>
        {
            captured = inv;
            extraAction?.Invoke(inv);
            inv.Proceed();
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();
        return captured;
    }

    /// <summary>
    /// Invokes CastleInterceptorAdapter.Invoke() and captures the adapter without proceeding.
    /// </summary>
    private static IInvocation InvokeAdapterNoProceed(
        AspectContext context,
        AspectDelegate next,
        Action<IInvocation> action)
    {
        IInvocation captured = null!;
        var castleInterceptor = new InlineInterceptor(inv =>
        {
            captured = inv;
            action?.Invoke(inv);
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();
        return captured;
    }

    // ── Constructor Tests ───────────────────────────────────────────────

    [Fact]
    public void Constructor_NullContext_Throws()
    {
        var ctor = AdapterType.GetConstructor(new[] { typeof(AspectContext), typeof(AspectDelegate) })!;

        var ex = Assert.Throws<TargetInvocationException>(() =>
            ctor.Invoke(new object[] { null!, (AspectDelegate)(_ => Task.CompletedTask) }));
        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    [Fact]
    public void Constructor_NullNext_Throws()
    {
        var context = CreateRealAspectContext(out _);
        var ctor = AdapterType.GetConstructor(new[] { typeof(AspectContext), typeof(AspectDelegate) })!;

        var ex = Assert.Throws<TargetInvocationException>(() =>
            ctor.Invoke(new object[] { context, null! }));
        Assert.IsType<ArgumentNullException>(ex.InnerException);
    }

    // ── Property Tests ──────────────────────────────────────────────────

    [Fact]
    public void Arguments_Returns_Context_Parameters()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.Equal(2, inv.Arguments.Length);
        Assert.Equal(10, inv.Arguments[0]);
        Assert.Equal(20, inv.Arguments[1]);
    }

    [Fact]
    public void GenericArguments_Returns_GenericArgs_For_Generic_Method()
    {
        var context = CreateGenericAspectContext(useGenericMethod: true);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.NotNull(inv.GenericArguments);
        Assert.Single(inv.GenericArguments);
        Assert.Equal(typeof(string), inv.GenericArguments[0]);
    }

    [Fact]
    public void GenericArguments_Returns_Empty_For_NonGeneric_Method()
    {
        var context = CreateGenericAspectContext(useGenericMethod: false);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.NotNull(inv.GenericArguments);
        Assert.Empty(inv.GenericArguments);
    }

    [Fact]
    public void InvocationTarget_Returns_Implementation()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.NotNull(inv.InvocationTarget);
        Assert.IsType<GreetService>(inv.InvocationTarget);
    }

    [Fact]
    public void Method_Returns_ServiceMethod()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.Equal("Add", inv.Method.Name);
    }

    [Fact]
    public void MethodInvocationTarget_Returns_ImplementationMethod()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.Equal("Add", inv.MethodInvocationTarget.Name);
    }

    [Fact]
    public void Proxy_Returns_Context_Proxy()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        // Proxy is the proxy object from the backing InvocationAspectContext
        Assert.NotNull(inv.Proxy);
    }

    [Fact]
    public void ReturnValue_Get_Returns_Context_ReturnValue()
    {
        var context = CreateRealAspectContext(out _);
        // The context has ReturnValue=30 from the backing invocation
        AspectDelegate next = ctx => Task.CompletedTask;

        IInvocation captured = null!;
        var castleInterceptor = new InlineInterceptor(inv =>
        {
            captured = inv;
            // Don't proceed - just read ReturnValue
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();

        Assert.Equal(30, captured.ReturnValue);
    }

    [Fact]
    public void ReturnValue_Set_Updates_Context_ReturnValue()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        IInvocation captured = null!;
        var castleInterceptor = new InlineInterceptor(inv =>
        {
            captured = inv;
            inv.ReturnValue = 99;
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();

        Assert.Equal(99, context.ReturnValue);
    }

    [Fact]
    public void TargetType_Returns_Implementation_Type()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.Equal(typeof(GreetService), inv.TargetType);
    }

    [Fact]
    public void TargetType_Falls_Back_To_DeclaringType_When_Implementation_Null()
    {
        // Create a context where Implementation returns null
        // We achieve this by creating a custom adapter invocation where the backing
        // Castle invocation's InvocationTarget is null (interface-only proxy)
        var generator = new CastleProxyGenerator();
        IInvocation capturedInv = null!;
        var interceptor = new InlineInterceptor(inv =>
        {
            capturedInv = inv;
            // For interface proxy without target, InvocationTarget might be null
            inv.ReturnValue = "mock";
        });
        var proxy = generator.CreateInterfaceProxyWithoutTarget<IGreetService>(interceptor);
        proxy.Greet("test");

        // Now capturedInv.InvocationTarget is null for without-target proxies
        var ctor = InvocationAspectContextType.GetConstructor(new[] { typeof(IInvocation) })!;
        var ctx = (AspectContext)ctor.Invoke(new object[] { capturedInv });

        // Now use this context with the adapter
        AspectDelegate next = c => Task.CompletedTask;
        var inv = InvokeAdapterAndCapture(ctx, next);

        // TargetType should fall back to DeclaringType
        Assert.NotNull(inv.TargetType);
    }

    [Fact]
    public void GetArgumentValue_Returns_Correct_Value()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.Equal(10, inv.GetArgumentValue(0));
        Assert.Equal(20, inv.GetArgumentValue(1));
    }

    [Fact]
    public void SetArgumentValue_Modifies_Context_Parameters()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        InvokeAdapterNoProceed(context, next, inv =>
        {
            inv.SetArgumentValue(0, 100);
            inv.SetArgumentValue(1, 200);
        });

        Assert.Equal(100, context.Parameters[0]);
        Assert.Equal(200, context.Parameters[1]);
    }

    [Fact]
    public void GetConcreteMethod_Returns_ImplementationMethod()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.Equal(inv.MethodInvocationTarget, inv.GetConcreteMethod());
    }

    [Fact]
    public void GetConcreteMethodInvocationTarget_Returns_ImplementationMethod()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var inv = InvokeAdapterAndCapture(context, next);

        Assert.Equal(inv.MethodInvocationTarget, inv.GetConcreteMethodInvocationTarget());
    }

    // ── Proceed Tests ───────────────────────────────────────────────────

    [Fact]
    public void Proceed_Calls_Next_Delegate()
    {
        var context = CreateRealAspectContext(out _);
        var nextCalled = false;
        AspectDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        InvokeAdapterAndCapture(context, next);

        Assert.True(nextCalled);
    }

    [Fact]
    public void Proceed_Called_Twice_Throws_InvalidOperationException()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            inv.Proceed();
            Assert.Throws<InvalidOperationException>(() => inv.Proceed());
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();
    }

    [Fact]
    public void Proceed_Synchronous_Faulted_Task_Propagates_Exception()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = _ => Task.FromException(new InvalidOperationException("test error"));

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            Assert.Throws<InvalidOperationException>(() => inv.Proceed());
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();
    }

    [Fact]
    public async Task Proceed_Async_Stores_AsyncResult()
    {
        var context = CreateRealAspectContext(out _);
        var tcs = new TaskCompletionSource<bool>();
        AspectDelegate next = _ => tcs.Task;

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            inv.Proceed(); // This stores the async task
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        var invokeTask = adapter.Invoke(context, next);

        Assert.False(invokeTask.IsCompleted);

        tcs.SetResult(true);
        await invokeTask;
    }

    [Fact]
    public void CastleInterceptorAdapter_No_AsyncResult_Returns_CompletedTask()
    {
        // When the interceptor does NOT call Proceed, AsyncResult is null -> Task.CompletedTask
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = _ => Task.CompletedTask;

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            // Intentionally not calling Proceed
            inv.ReturnValue = 42;
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        var task = adapter.Invoke(context, next);

        Assert.True(task.IsCompleted);
    }

    // ── CaptureProceedInfo Tests ────────────────────────────────────────

    [Fact]
    public void CaptureProceedInfo_Invoke_Calls_Proceed()
    {
        var context = CreateRealAspectContext(out _);
        var nextCalled = false;
        AspectDelegate next = ctx =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        };

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            var info = inv.CaptureProceedInfo();
            info.Invoke();
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();

        Assert.True(nextCalled);
    }

    [Fact]
    public void CaptureProceedInfo_Double_Invoke_Throws()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx => Task.CompletedTask;

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            var info = inv.CaptureProceedInfo();
            info.Invoke();
            Assert.Throws<InvalidOperationException>(() => info.Invoke());
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        adapter.Invoke(context, next).GetAwaiter().GetResult();
    }

    // ── CastleInterceptorAdapter Constructor Tests ──────────────────────

    [Fact]
    public void CastleInterceptorAdapter_Null_Constructor_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new CastleInterceptorAdapter(null!));
    }

    // ── CastleInterceptorAdapter async return path ──────────────────────

    [Fact]
    public async Task CastleInterceptorAdapter_Invoke_Returns_AsyncResult_When_Present()
    {
        var context = CreateRealAspectContext(out _);
        var tcs = new TaskCompletionSource<string>();
        AspectDelegate next = ctx =>
        {
            ctx.ReturnValue = tcs.Task;
            return tcs.Task;
        };

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            inv.Proceed();
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        var task = adapter.Invoke(context, next);

        Assert.False(task.IsCompleted);
        tcs.SetResult("async-done");
        await task;
    }

    [Fact]
    public void CastleInterceptorAdapter_Invoke_Returns_CompletedTask_When_Sync()
    {
        var context = CreateRealAspectContext(out _);
        AspectDelegate next = ctx =>
        {
            ctx.ReturnValue = "sync";
            return Task.CompletedTask;
        };

        var castleInterceptor = new InlineInterceptor(inv =>
        {
            inv.Proceed();
        });
        var adapter = new CastleInterceptorAdapter(castleInterceptor);
        var task = adapter.Invoke(context, next);

        Assert.True(task.IsCompleted);
        Assert.Equal("sync", context.ReturnValue);
    }

    // ── Helper ──────────────────────────────────────────────────────────

    private sealed class InlineInterceptor : Castle.DynamicProxy.IInterceptor
    {
        private readonly Action<IInvocation> _action;
        public InlineInterceptor(Action<IInvocation> action) => _action = action;
        public void Intercept(IInvocation invocation) => _action(invocation);
    }
}
