using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;

namespace AspectCore.Extensions.AspectScope
{
    //[NonAspect]
    //internal sealed class ScopeAspectContext : AspectContext, IDisposable
    //{
    //    private static readonly AsyncLocal<AspectContext> CurrentRuntimeContext = new AsyncLocal<AspectContext>();
    //    private readonly IAspectScheduler _aspectContextScheduler;
    //    private readonly AspectContext _runtimeContext;

    //    internal int Id { get; set; }

    //    internal AspectContext RuntimeContext
    //    {
    //        get => CurrentRuntimeContext.Value;
    //        private set => CurrentRuntimeContext.Value = value;
    //    }

    //    public override IServiceProvider ServiceProvider => _runtimeContext.ServiceProvider;

    //    public override IDictionary<string, object> AdditionalData => _runtimeContext.AdditionalData;

    //    public override object ReturnValue { get => _runtimeContext.ReturnValue; set => _runtimeContext.ReturnValue = value; }

    //    public override MethodInfo ServiceMethod => _runtimeContext.ServiceMethod;

    //    public override object[] Parameters => _runtimeContext.Parameters;

    //    public override MethodInfo ProxyMethod => _runtimeContext.ProxyMethod;

    //    public override object Implementation => _runtimeContext.Implementation;

    //    public override MethodInfo ImplementationMethod => _runtimeContext.ImplementationMethod;

    //    public override object Proxy => _runtimeContext.Proxy;

    //    internal ScopeAspectContext(AspectContext runtimeContext, IAspectScheduler aspectContextScheduler)
    //    {
    //        _aspectContextScheduler = aspectContextScheduler;
    //        if (!_aspectContextScheduler.TryEnter(this))
    //        {
    //            throw new InvalidOperationException("Error occurred in the schedule AspectContext.");
    //        }
    //        RuntimeContext = _runtimeContext = runtimeContext;
    //        var trace = new System.Diagnostics.StackTrace();
    //        var frames = trace.GetFrames();
    //    }

    //    //protected override void Dispose(bool disposing)
    //    //{
    //    //    _runtimeContext.Dispose();
    //    //    _aspectContextScheduler.Release(this);
    //    //    RuntimeContext = null;
    //    //}

    //    public override Task Break()
    //    {
    //        return _runtimeContext.Break();
    //    }

    //    public override Task Complete()
    //    {
    //        return _runtimeContext.Complete();
    //    }

    //    public override Task Invoke(AspectDelegate next)
    //    {
    //        return _runtimeContext.Invoke(next);
    //    }

    //    public void Dispose()
    //    {
    //        (_runtimeContext as
    //             IDisposable).Dispose();
    //        _aspectContextScheduler.Release(this);
    //        RuntimeContext = null;
    //    }
    //}
}