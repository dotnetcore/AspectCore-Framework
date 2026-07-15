using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.E2E.Tests.Fixtures;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// E2E tests for parameter aspects: EnableParameterAspect configuration,
/// parameter interceptor invocation, return parameter interception, and
/// parameter aspect combined with method interception. Real DI container,
/// real proxy pipeline — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class ParameterAspectScenarios
{
    [Fact]
    public void EnableParameterAspect_ParameterInterceptor_IsInvoked()
    {
        using var host = new TestHost();
        host.Add<IParamAspectService, ParamAspectService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamAspectService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamAspectService)));
        });

        var result = service.Double(5);
        Assert.Equal(10, result);
        // The parameter interceptor on the 'value' parameter was invoked.
        Assert.Contains("ParamInterceptor:value", InterceptorLog.Entries);
    }

    [Fact]
    public void EnableParameterAspect_MultipleParameters_BothIntercepted()
    {
        using var host = new TestHost();
        host.Add<IParamAspectService, ParamAspectService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamAspectService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamAspectService)));
        });

        var result = service.Add(3, 4);
        Assert.Equal(7, result);
        Assert.Contains("ParamInterceptor:a", InterceptorLog.Entries);
        Assert.Contains("ParamInterceptor:b", InterceptorLog.Entries);
    }

    [Fact]
    public void EnableParameterAspect_ReturnParameterInterceptor_IsInvoked()
    {
        using var host = new TestHost();
        host.Add<IParamAspectService, ParamAspectService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamAspectService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamAspectService)));
        });

        var result = service.GetValue(42);
        Assert.Equal(42, result);
        Assert.Contains("ReturnInterceptor", InterceptorLog.Entries);
    }

    [Fact]
    public void EnableParameterAspect_WithMethodInterceptor_BothExecute()
    {
        using var host = new TestHost();
        host.Add<IParamAspectService, ParamAspectService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamAspectService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamAspectService)));
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("MethodInterceptor.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("MethodInterceptor.After");
            }, Predicates.Implement(typeof(IParamAspectService)));
        });

        var result = service.Double(5);
        Assert.Equal(10, result);
        // Both the method interceptor and the parameter interceptor execute.
        Assert.Contains("MethodInterceptor.Before", InterceptorLog.Entries);
        Assert.Contains("ParamInterceptor:value", InterceptorLog.Entries);
        Assert.Contains("MethodInterceptor.After", InterceptorLog.Entries);
    }

    [Fact]
    public void EnableParameterAspect_StringParameter_Intercepted()
    {
        using var host = new TestHost();
        host.Add<IParamAspectService, ParamAspectService>();

        InterceptorLog.Clear();
        var service = host.Resolve<IParamAspectService>(config =>
        {
            config.EnableParameterAspect(Predicates.Implement(typeof(IParamAspectService)));
        });

        var result = service.Echo("hello");
        Assert.Equal("hello", result);
        Assert.Contains("ParamInterceptor:input", InterceptorLog.Entries);
    }

    /// <summary>
    /// Service interface with parameter-level interceptors applied directly
    /// on the interface method parameters (so the proxy can read them).
    /// </summary>
    public interface IParamAspectService
    {
        int Double([LogParamInterceptor("value")] int value);
        int Add(
            [LogParamInterceptor("a")] int a,
            [LogParamInterceptor("b")] int b);
        [return: LogReturnInterceptor]
        int GetValue([LogParamInterceptor("value")] int value);
        string Echo([LogParamInterceptor("input")] string input);
    }

    /// <summary>
    /// Real implementation of IParamAspectService. Parameter interceptors are
    /// defined on the interface so the proxy can read them.
    /// </summary>
    public class ParamAspectService : IParamAspectService
    {
        public int Double(int value) => value * 2;

        public int Add(int a, int b) => a + b;

        public int GetValue(int value) => value;

        public string Echo(string input) => input;
    }

    /// <summary>
    /// Parameter interceptor that logs the parameter name to InterceptorLog.
    /// </summary>
    public class LogParamInterceptor : ParameterInterceptorAttribute
    {
        private readonly string _name;

        public LogParamInterceptor(string name)
        {
            _name = name;
        }

        public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
        {
            InterceptorLog.Entries.Add($"ParamInterceptor:{_name}");
            return next(context);
        }
    }

    /// <summary>
    /// Return parameter interceptor that logs to InterceptorLog.
    /// </summary>
    public class LogReturnInterceptor : ReturnParameterInterceptorAttribute
    {
        public override Task Invoke(ParameterAspectContext context, ParameterAspectDelegate next)
        {
            InterceptorLog.Entries.Add("ReturnInterceptor");
            return next(context);
        }
    }
}
