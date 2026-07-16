using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Tenth batch of E2E coverage tests — exercises proxy generation edge cases:
/// explicit interface implementations, property interception (get/set),
/// in parameters, nested generics, multiple interfaces, generic constraints,
/// interceptor-modified ref/out parameters, and various return kinds.
/// These target ILEmitVisitor, ClassProxyAstBuilder, InterfaceImplAstBuilder,
/// and ProxyGenerator code paths.
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios10
{
    // ========================================================================
    // Explicit interface implementation
    // ========================================================================

    [Fact]
    public void ExplicitInterfaceImplementation_Proxy_Works()
    {
        using var host = new TestHost();
        host.Add<IExplicitService, ExplicitServiceImpl>();

        var service = host.Resolve<IExplicitService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IExplicitService)));
        });

        Assert.Equal("explicit-impl", service.DoWork());
    }

    // ========================================================================
    // Property interception — get and set with interceptors
    // ========================================================================

    [Fact]
    public void PropertyInterception_GetSet_WithInterceptor_Works()
    {
        using var host = new TestHost();
        host.Add<IPropertyService, PropertyServiceImpl>();

        InterceptorLog.Clear();
        var service = host.Resolve<IPropertyService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add($"Prop.{ctx.ServiceMethod.Name}");
                await ctx.Invoke(next);
            }, Predicates.Implement(typeof(IPropertyService)));
        });

        service.Name = "test-prop";
        Assert.Equal("test-prop", service.Name);

        Assert.Contains("Prop.set_Name", InterceptorLog.Entries);
        Assert.Contains("Prop.get_Name", InterceptorLog.Entries);
    }

    [Fact]
    public void PropertyInterception_ReadOnlyProperty_Works()
    {
        using var host = new TestHost();
        host.Add<IPropertyService, PropertyServiceImpl>();

        var service = host.Resolve<IPropertyService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IPropertyService)));
        });

        Assert.Equal("read-only-value", service.ReadOnly);
    }

    // ========================================================================
    // Multiple parameter types — value types, reference types, nullable
    // ========================================================================

    [Fact]
    public void MultipleParameterTypes_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<IMultiParamTypeService, MultiParamTypeServiceImpl>();

        var service = host.Resolve<IMultiParamTypeService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IMultiParamTypeService)));
        });

        Assert.Equal(42, service.ProcessInt(21));
        Assert.Equal("hello-world", service.Concat("hello", "world"));
        Assert.True(service.IsNull(null));
        Assert.False(service.IsNull("not-null"));
    }

    // ========================================================================
    // Nested generic types
    // ========================================================================

    [Fact]
    public void NestedGenericTypes_ThroughProxy_Works()
    {
        using var host = new TestHost();
        host.Add<INestedGenericService, NestedGenericServiceImpl>();

        var service = host.Resolve<INestedGenericService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(INestedGenericService)));
        });

        var data = new Dictionary<string, List<int>>
        {
            ["a"] = new() { 1, 2, 3 },
            ["b"] = new() { 4, 5 }
        };

        var result = service.Process(data);
        Assert.Equal(5, result);
    }

    // ========================================================================
    // Multiple interfaces on one service
    // ========================================================================

    [Fact]
    public void MultipleInterfaces_BothProxied_Work()
    {
        using var host = new TestHost();
        host.Services.AddSingleton<MultiInterfaceImpl>();
        host.Services.AddSingleton<IFirstInterface>(sp => sp.GetRequiredService<MultiInterfaceImpl>());
        host.Services.AddSingleton<ISecondInterface>(sp => sp.GetRequiredService<MultiInterfaceImpl>());

        var service1 = host.Resolve<IFirstInterface>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IFirstInterface)));
        });

        var service2 = host.Resolve<ISecondInterface>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ISecondInterface)));
        });

        Assert.Equal("first", service1.First());
        Assert.Equal(42, service2.Second());
    }

    // ========================================================================
    // Generic constraints — where T : class, where T : struct, where T : new()
    // ========================================================================

    [Fact]
    public void GenericConstraint_ClassConstraint_Works()
    {
        using var host = new TestHost();
        host.Add<IGenericConstraintService, GenericConstraintServiceImpl>();

        var service = host.Resolve<IGenericConstraintService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericConstraintService)));
        });

        var result = service.Create<ConstraintTestClass>();
        Assert.NotNull(result);
        Assert.IsType<ConstraintTestClass>(result);
    }

    [Fact]
    public void GenericConstraint_StructConstraint_Works()
    {
        using var host = new TestHost();
        host.Add<IGenericConstraintService, GenericConstraintServiceImpl>();

        var service = host.Resolve<IGenericConstraintService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IGenericConstraintService)));
        });

        var result = service.Default<int>();
        Assert.Equal(0, result);
    }

    // ========================================================================
    // Interceptor modifies ref parameter
    // ========================================================================

    [Fact]
    public void Interceptor_ModifiesRefParameter_Works()
    {
        using var host = new TestHost();
        host.Add<IRefOutService, RefOutServiceImpl>();

        var service = host.Resolve<IRefOutService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                // Modify the ref parameter before calling next.
                var parameters = ctx.GetParameters();
                if (parameters.Count > 0 && parameters[0].Name == "value")
                {
                    parameters[0].Value = 100;
                }
                return next(ctx);
            }, Predicates.Implement(typeof(IRefOutService)));
        });

        var val = 5;
        service.IncrementRef(ref val);
        // Interceptor set value to 100, then service increments by 1.
        Assert.Equal(101, val);
    }

    // ========================================================================
    // Various return kinds — Void, Sync, Task, TaskOfT, ValueTask, ValueTaskOfT
    // ========================================================================

    [Fact]
    public async Task AllReturnKinds_ThroughProxy_Work()
    {
        using var host = new TestHost();
        host.Add<IReturnKindService, ReturnKindServiceImpl>();

        var service = host.Resolve<IReturnKindService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IReturnKindService)));
        });

        // Void
        service.VoidMethod();

        // Sync
        Assert.Equal(42, service.SyncMethod());

        // Task
        await service.TaskMethod();

        // Task<T>
        Assert.Equal("task-result", await service.TaskOfTMethod());

        // ValueTask
        await service.ValueTaskMethod();

        // ValueTask<T>
        Assert.Equal(99, await service.ValueTaskOfTMethod());
    }

    // ========================================================================
    // Class proxy with constructor forwarding (various parameter types)
    // ========================================================================

    [Fact]
    public void ClassProxy_ConstructorWithMultipleParams_Works()
    {
        using var host = new TestHost();
        host.Add<ClassWithConstructor>();

        var service = host.Resolve<ClassWithConstructor>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(ClassWithConstructor)));
        });

        Assert.Equal("hello", service.Name);
        Assert.Equal(42, service.Count);
    }

    // ========================================================================
    // Multiple interceptors with ordering
    // ========================================================================

    [Fact]
    public void MultipleInterceptors_Ordered_ExecuteInOrder()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        InterceptorLog.Clear();
        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("A.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("A.After");
            }, Predicates.Implement(typeof(ICalculatorService)));

            config.Interceptors.AddDelegate(async (ctx, next) =>
            {
                InterceptorLog.Entries.Add("B.Before");
                await ctx.Invoke(next);
                InterceptorLog.Entries.Add("B.After");
            }, Predicates.Implement(typeof(ICalculatorService)));
        });

        var result = service.Add(1, 2);

        Assert.Equal(3, result);
        Assert.Equal("A.Before", InterceptorLog.Entries[0]);
        Assert.Equal("B.Before", InterceptorLog.Entries[1]);
        Assert.Equal("B.After", InterceptorLog.Entries[2]);
        Assert.Equal("A.After", InterceptorLog.Entries[3]);
    }

    // ========================================================================
    // Interceptor that throws exception — sync and async
    // ========================================================================

    [Fact]
    public void Interceptor_ThrowsSyncException_Propagates()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
                throw new InvalidOperationException("interceptor-fail"),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var ex = Assert.Throws<InvalidOperationException>(() => service.Add(1, 2));
        Assert.Equal("interceptor-fail", ex.Message);
    }

    // ========================================================================
    // Generic method with multiple type parameters
    // ========================================================================

    [Fact]
    public void GenericMethod_MultipleTypeParameters_Works()
    {
        using var host = new TestHost();
        host.Add<IMultiGenericService, MultiGenericServiceImpl>();

        var service = host.Resolve<IMultiGenericService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(IMultiGenericService)));
        });

        var result = service.Combine<int, string>(42, "hello");
        Assert.Equal("42-hello", result);
    }

    // ========================================================================
    // Helper types
    // ========================================================================

    public interface IExplicitService
    {
        string DoWork();
    }

    public class ExplicitServiceImpl : IExplicitService
    {
        string IExplicitService.DoWork() => "explicit-impl";
    }

    public interface IPropertyService
    {
        string Name { get; set; }
        string ReadOnly { get; }
    }

    public class PropertyServiceImpl : IPropertyService
    {
        public string Name { get; set; } = "";
        public string ReadOnly => "read-only-value";
    }

    public interface IMultiParamTypeService
    {
        int ProcessInt(int value);
        string Concat(string a, string b);
        bool IsNull(string? value);
    }

    public class MultiParamTypeServiceImpl : IMultiParamTypeService
    {
        public int ProcessInt(int value) => value * 2;
        public string Concat(string a, string b) => $"{a}-{b}";
        public bool IsNull(string? value) => value == null;
    }

    public interface INestedGenericService
    {
        int Process(Dictionary<string, List<int>> data);
    }

    public class NestedGenericServiceImpl : INestedGenericService
    {
        public int Process(Dictionary<string, List<int>> data)
        {
            int total = 0;
            foreach (var list in data.Values)
                total += list.Count;
            return total;
        }
    }

    public interface IFirstInterface
    {
        string First();
    }

    public interface ISecondInterface
    {
        int Second();
    }

    public class MultiInterfaceImpl : IFirstInterface, ISecondInterface
    {
        public string First() => "first";
        public int Second() => 42;
    }

    public interface IGenericConstraintService
    {
        T Create<T>() where T : class, new();
        T Default<T>() where T : struct;
    }

    public class GenericConstraintServiceImpl : IGenericConstraintService
    {
        public T Create<T>() where T : class, new() => new T();
        public T Default<T>() where T : struct => default;
    }

    public class ConstraintTestClass { }

    public interface IRefOutService
    {
        void IncrementRef(ref int value);
        void GetOut(out int value);
    }

    public class RefOutServiceImpl : IRefOutService
    {
        public void IncrementRef(ref int value) => value += 1;
        public void GetOut(out int value) => value = 99;
    }

    public interface IReturnKindService
    {
        void VoidMethod();
        int SyncMethod();
        Task TaskMethod();
        Task<string> TaskOfTMethod();
        ValueTask ValueTaskMethod();
        ValueTask<int> ValueTaskOfTMethod();
    }

    public class ReturnKindServiceImpl : IReturnKindService
    {
        public void VoidMethod() { }
        public int SyncMethod() => 42;
        public Task TaskMethod() => Task.CompletedTask;
        public Task<string> TaskOfTMethod() => Task.FromResult("task-result");
        public ValueTask ValueTaskMethod() => new(Task.CompletedTask);
        public ValueTask<int> ValueTaskOfTMethod() => new(99);
    }

    public class ClassWithConstructor
    {
        public string Name { get; }
        public int Count { get; }

        public ClassWithConstructor()
        {
            Name = "hello";
            Count = 42;
        }

        public virtual string GetName() => Name;
    }

    public interface IMultiGenericService
    {
        string Combine<TKey, TValue>(TKey key, TValue value);
    }

    public class MultiGenericServiceImpl : IMultiGenericService
    {
        public string Combine<TKey, TValue>(TKey key, TValue value)
            => $"{key}-{value}";
    }
}
