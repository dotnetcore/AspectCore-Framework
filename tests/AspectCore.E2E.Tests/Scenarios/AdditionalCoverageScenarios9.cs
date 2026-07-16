using System;
using System.Linq;
using System.Reflection;
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
/// Ninth batch of E2E coverage tests — exercises ParameterCollection (indexers,
/// GetValues, enumeration, edge cases) and ReflectionUtils public methods
/// (IsProxy, IsProxyType, CanInherited, IsNonAspect, IsReturnTask,
/// IsReturnValueTask, IsVisibleAndVirtual, GetMethodBySignature). All tests
/// use real proxies and real reflection — no mocks.
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios9
{
    // ========================================================================
    // ParameterCollection tests — accessed through AspectContext.GetParameters()
    // ========================================================================

    [Fact]
    public void ParameterCollection_Indexer_ByName_SingleParameter_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.SingleParam("hello");

        Assert.NotNull(captured);
        Assert.Single(captured!);

        // Indexer by name — single parameter fast path.
        var param = captured["value"];
        Assert.Equal("value", param.Name);
        Assert.Equal("hello", param.Value);
        Assert.Equal(typeof(string), param.Type);
    }

    [Fact]
    public void ParameterCollection_Indexer_ByName_MultipleParameters_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.MultiParam(42, "world", 3.14);

        Assert.NotNull(captured);
        Assert.Equal(3, captured!.Count);

        // Indexer by name — multiple parameters loop path.
        Assert.Equal(42, captured["a"].Value);
        Assert.Equal("world", captured["b"].Value);
        Assert.Equal(3.14, captured["c"].Value);
    }

    [Fact]
    public void ParameterCollection_Indexer_ByName_NotFound_Throws()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.SingleParam("test");

        Assert.NotNull(captured);
        Assert.Throws<InvalidOperationException>(() => _ = captured!["nonexistent"]);
    }

    [Fact]
    public void ParameterCollection_Indexer_ByName_Null_Throws()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.SingleParam("test");

        Assert.NotNull(captured);
        Assert.Throws<ArgumentNullException>(() => _ = captured![null!]);
    }

    [Fact]
    public void ParameterCollection_Indexer_ByInt_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.MultiParam(1, "two", 3.0);

        Assert.NotNull(captured);
        Assert.Equal(1, captured![0].Value);
        Assert.Equal("two", captured[1].Value);
        Assert.Equal(3.0, captured[2].Value);
    }

    [Fact]
    public void ParameterCollection_Indexer_ByInt_OutOfRange_Throws()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.SingleParam("test");

        Assert.NotNull(captured);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = captured![5]);
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = captured![-1]);
    }

    [Fact]
    public void ParameterCollection_GetValues_ReturnsAllValues()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.MultiParam(10, "hello", 1.5);

        Assert.NotNull(captured);
        var values = captured!.GetValues();
        Assert.Equal(3, values.Length);
        Assert.Equal(10, values[0]);
        Assert.Equal("hello", values[1]);
        Assert.Equal(1.5, values[2]);
    }

    [Fact]
    public void ParameterCollection_GetValues_Empty_ReturnsEmptyArray()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.NoParam();

        Assert.NotNull(captured);
        Assert.Empty(captured!);
        var values = captured!.GetValues();
        Assert.Empty(values);
    }

    [Fact]
    public void ParameterCollection_GetEnumerator_Works()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        service.MultiParam(1, "two", 3.0);

        Assert.NotNull(captured);

        var names = captured!.Select(p => p.Name).ToList();
        Assert.Equal(new[] { "a", "b", "c" }, names);
    }

    [Fact]
    public void ParameterCollection_RefParameter_HasIsRefTrue()
    {
        using var host = new TestHost();
        host.Add<IParameterTestService, ParameterTestService>();

        ParameterCollection? captured = null;
        var service = host.Resolve<IParameterTestService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) =>
            {
                captured = ctx.GetParameters();
                return next(ctx);
            }, Predicates.Implement(typeof(IParameterTestService)));
        });

        var val = 5;
        service.RefParam(ref val);

        Assert.NotNull(captured);
        Assert.True(captured![0].IsRef);
        Assert.Equal(typeof(int).MakeByRefType(), captured[0].Type);
        Assert.Equal(typeof(int), captured[0].RawType);
    }

    // ========================================================================
    // ReflectionUtils tests — public methods
    // ========================================================================

    [Fact]
    public void ReflectionUtils_IsProxy_TrueForProxiedService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        Assert.True(service.IsProxy());
    }

    [Fact]
    public void ReflectionUtils_IsProxy_FalseForNonProxiedService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        // No interceptor configured → no proxy generated.
        var service = host.Resolve<ICalculatorService>();

        Assert.False(service.IsProxy());
    }

    [Fact]
    public void ReflectionUtils_IsProxy_FalseForNull()
    {
        object? nullObj = null;
        Assert.False(nullObj!.IsProxy());
    }

    [Fact]
    public void ReflectionUtils_IsProxyType_TrueForProxyType()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>();

        var service = host.Resolve<ICalculatorService>(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var typeInfo = service.GetType().GetTypeInfo();
        Assert.True(typeInfo.IsProxyType());
    }

    [Fact]
    public void ReflectionUtils_CanInherited_TrueForPublicClass()
    {
        Assert.True(typeof(PublicInheritableClass).GetTypeInfo().CanInherited());
    }

    [Fact]
    public void ReflectionUtils_CanInherited_FalseForSealedClass()
    {
        Assert.False(typeof(SealedClass).GetTypeInfo().CanInherited());
    }

    [Fact]
    public void ReflectionUtils_CanInherited_FalseForValueType()
    {
        Assert.False(typeof(int).GetTypeInfo().CanInherited());
        Assert.False(typeof(StructType).GetTypeInfo().CanInherited());
    }

    [Fact]
    public void ReflectionUtils_CanInherited_FalseForEnum()
    {
        Assert.False(typeof(TestEnum).GetTypeInfo().CanInherited());
    }

    [Fact]
    public void ReflectionUtils_IsNonAspect_TrueForNonAspectType()
    {
        Assert.True(typeof(NonAspectType3).GetTypeInfo().IsNonAspect());
    }

    [Fact]
    public void ReflectionUtils_IsNonAspect_FalseForNormalType()
    {
        Assert.False(typeof(PublicInheritableClass).GetTypeInfo().IsNonAspect());
    }

    [Fact]
    public void ReflectionUtils_IsNonAspect_MethodInfo_TrueForNonAspectMethod()
    {
        var method = typeof(NonAspectType3).GetMethod(nameof(NonAspectType3.DoWork))!;
        Assert.True(method.IsNonAspect());
    }

    [Fact]
    public void ReflectionUtils_IsNonAspect_MethodInfo_FalseForNormalMethod()
    {
        var method = typeof(PublicInheritableClass).GetMethod(nameof(PublicInheritableClass.DoWork))!;
        Assert.False(method.IsNonAspect());
    }

    [Fact]
    public void ReflectionUtils_IsReturnTask_TrueForTaskOfT()
    {
        var method = typeof(IReflectionTestService).GetMethod(nameof(IReflectionTestService.GetTaskAsync))!;
        Assert.True(method.IsReturnTask());
    }

    [Fact]
    public void ReflectionUtils_IsReturnTask_FalseForPlainTask()
    {
        var method = typeof(IReflectionTestService).GetMethod(nameof(IReflectionTestService.GetPlainTaskAsync))!;
        Assert.False(method.IsReturnTask());
    }

    [Fact]
    public void ReflectionUtils_IsReturnValueTask_TrueForValueTaskOfT()
    {
        var method = typeof(IReflectionTestService).GetMethod(nameof(IReflectionTestService.GetValueTaskAsync))!;
        Assert.True(method.IsReturnValueTask());
    }

    [Fact]
    public void ReflectionUtils_IsReturnValueTask_FalseForPlainValueTask()
    {
        var method = typeof(IReflectionTestService).GetMethod(nameof(IReflectionTestService.GetPlainValueTaskAsync))!;
        Assert.False(method.IsReturnValueTask());
    }

    [Fact]
    public void ReflectionUtils_IsVisibleAndVirtual_TrueForVirtualMethod()
    {
        var method = typeof(VirtualPropertyClass).GetMethod(nameof(VirtualPropertyClass.VirtualMethod))!;
        Assert.True(method.IsVisibleAndVirtual());
    }

    [Fact]
    public void ReflectionUtils_IsVisibleAndVirtual_FalseForStaticMethod()
    {
        var method = typeof(VirtualPropertyClass).GetMethod(nameof(VirtualPropertyClass.StaticMethod))!;
        Assert.False(method.IsVisibleAndVirtual());
    }

    [Fact]
    public void ReflectionUtils_IsVisibleAndVirtual_FalseForNonVirtualMethod()
    {
        var method = typeof(VirtualPropertyClass).GetMethod(nameof(VirtualPropertyClass.NonVirtualMethod))!;
        Assert.False(method.IsVisibleAndVirtual());
    }

    [Fact]
    public void ReflectionUtils_IsVisibleAndVirtual_Property_TrueForVirtualProperty()
    {
        var property = typeof(VirtualPropertyClass).GetProperty(nameof(VirtualPropertyClass.VirtualProp))!;
        Assert.True(property.IsVisibleAndVirtual());
    }

    [Fact]
    public void ReflectionUtils_GetMethodBySignature_FindsMatchingMethod()
    {
        var typeInfo = typeof(CalculatorService).GetTypeInfo();
        var sourceMethod = typeof(ICalculatorService).GetMethod(nameof(ICalculatorService.Add))!;

        var found = typeInfo.GetMethodBySignature(sourceMethod);
        Assert.NotNull(found);
        Assert.Equal(nameof(ICalculatorService.Add), found!.Name);
    }

    // ========================================================================
    // Helper types
    // ========================================================================

    public interface IParameterTestService
    {
        void SingleParam(string value);
        void MultiParam(int a, string b, double c);
        void NoParam();
        void RefParam(ref int value);
    }

    public class ParameterTestService : IParameterTestService
    {
        public void SingleParam(string value) { }
        public void MultiParam(int a, string b, double c) { }
        public void NoParam() { }
        public void RefParam(ref int value) { value += 1; }
    }

    public class PublicInheritableClass
    {
        public void DoWork() { }
    }

    public sealed class SealedClass { }

    public struct StructType { public int Value; }

    public enum TestEnum { Value1, Value2 }

    [NonAspect]
    public class NonAspectType3
    {
        public void DoWork() { }
    }

    public interface IReflectionTestService
    {
        Task<int> GetTaskAsync();
        Task GetPlainTaskAsync();
        ValueTask<int> GetValueTaskAsync();
        ValueTask GetPlainValueTaskAsync();
    }

    public class VirtualPropertyClass
    {
        public virtual int VirtualProp { get; set; }
        public int NonVirtualProp { get; set; }

        public virtual void VirtualMethod() { }
        public static void StaticMethod() { }
        public void NonVirtualMethod() { }
    }
}
