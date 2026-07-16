using System;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Utils;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for NoSyncContextScope: verifying that async proxy method execution
/// can run within a NoSyncContextScope (no deadlock), that sync context is
/// properly restored after execution, and that return values are correctly
/// propagated. Real async methods, real synchronization context — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class NoSyncContextScopeScenarios
{
    [Fact]
    public void NoSyncContextScope_Run_Task_ExecutesTask()
    {
        var executed = false;
        var task = Task.Run(() => { executed = true; });

        NoSyncContextScope.Run(task);

        Assert.True(executed);
    }

    [Fact]
    public void NoSyncContextScope_Run_TaskOfT_ReturnsResult()
    {
        var task = Task.FromResult(42);

        var result = NoSyncContextScope.Run(task);

        Assert.Equal(42, result);
    }

    [Fact]
    public void NoSyncContextScope_Run_AsyncTask_ExecutesFully()
    {
        var task = RunAsyncWorkflow();

        var result = NoSyncContextScope.Run(task);

        Assert.Equal("done", result);
    }

    [Fact]
    public void NoSyncContextScope_Run_RestoresSyncContext_AfterExecution()
    {
        // Set a custom sync context before running.
        var originalContext = SynchronizationContext.Current;
        var customContext = new SynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(customContext);

        try
        {
            var task = Task.Delay(1);
            NoSyncContextScope.Run(task);

            // After execution, the original sync context is restored.
            Assert.Same(customContext, SynchronizationContext.Current);
        }
        finally
        {
            SynchronizationContext.SetSynchronizationContext(originalContext);
        }
    }

    [Fact]
    public void NoSyncContextScope_Run_ProxyMethod_NoDeadlock()
    {
        using var host = new TestHost();
        host.Add<IAsyncService, AsyncService>();

        var service = host.Resolve<IAsyncService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IAsyncService)));
        });

        // Running an async proxy method synchronously via NoSyncContextScope
        // must not deadlock and must return the correct result.
        var task = service.GetNameAsync();
        var result = NoSyncContextScope.Run(task);

        Assert.Equal("async-name", result);
    }

    [Fact]
    public void NoSyncContextScope_Run_ProxyMethodWithInterceptor_NoDeadlock()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("NoSync.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("NoSync.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        // Running an async proxy method with interceptors via NoSyncContextScope
        // must not deadlock and must execute all interceptors.
        var task = service.MultiplyAsync(3, 4);
        var result = NoSyncContextScope.Run(task);

        Assert.Equal(12, result);
        Assert.Contains("NoSync.Before", InterceptorLog.Entries);
        Assert.Contains("NoSync.After", InterceptorLog.Entries);
    }

    private static async Task<string> RunAsyncWorkflow()
    {
        await Task.Delay(1);
        var intermediate = await Task.FromResult("intermediate");
        await Task.Delay(1);
        return intermediate.Replace("intermediate", "done");
    }
}
