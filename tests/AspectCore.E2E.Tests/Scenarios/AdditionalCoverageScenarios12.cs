using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Twelfth batch of E2E coverage tests — exercises Predicates (ForNameSpace,
/// ForService with generics, ForMethod overloads, Implement with validation),
/// InterceptorCollectionExtensions (AddTyped, AddServiced, AddDelegate with
/// order, null checks), and generic service resolution through ServiceTable
/// (open generics, IEnumerable resolution, IManyEnumerable).
/// </summary>
public class AdditionalCoverageScenarios12
{
    // ========================================================================
    // Predicates — null checks and matching scenarios
    // ========================================================================

    [Fact]
    public void Predicates_ForNameSpace_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Predicates.ForNameSpace(null!));
    }

    [Fact]
    public void Predicates_ForService_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Predicates.ForService(null!));
    }

    [Fact]
    public void Predicates_ForMethod_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Predicates.ForMethod(null!));
    }

    [Fact]
    public void Predicates_ForMethod_ServiceAndMethod_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Predicates.ForMethod(null!, "method"));
        Assert.Throws<ArgumentNullException>(() => Predicates.ForMethod("service", null!));
    }

    [Fact]
    public void Predicates_Implement_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => Predicates.Implement(null!));
    }

    [Fact]
    public void Predicates_Implement_NonClassOrInterface_Throws()
    {
        Assert.Throws<ArgumentException>(() => Predicates.Implement(typeof(int)));
    }

    [Fact]
    public void Predicates_Implement_SealedType_Throws()
    {
        Assert.Throws<ArgumentException>(() => Predicates.Implement(typeof(SealedTestClass)));
    }

    [Fact]
    public void Predicates_ForNameSpace_MatchesNamespace()
    {
        var predicate = Predicates.ForNameSpace("AspectCore.E2E.Tests.Fixtures");
        var method = typeof(CalculatorService).GetMethod(nameof(CalculatorService.Add))!;
        Assert.True(predicate(method));
    }

    [Fact]
    public void Predicates_ForService_MatchesServiceName()
    {
        var predicate = Predicates.ForService(nameof(CalculatorService));
        var method = typeof(CalculatorService).GetMethod(nameof(CalculatorService.Add))!;
        Assert.True(predicate(method));
    }

    [Fact]
    public void Predicates_ForService_GenericType_StripsBacktick()
    {
        var predicate = Predicates.ForService("GenericRepository");
        var method = typeof(GenericRepository<ConstraintTestClass>).GetMethod(nameof(GenericRepository<ConstraintTestClass>.GetById))!;
        Assert.True(predicate(method));
    }

    [Fact]
    public void Predicates_ForMethod_MatchesMethodName()
    {
        var predicate = Predicates.ForMethod("Add");
        var method = typeof(CalculatorService).GetMethod(nameof(CalculatorService.Add))!;
        Assert.True(predicate(method));
    }

    [Fact]
    public void Predicates_ForMethod_ServiceAndMethod_BothMustMatch()
    {
        var predicate = Predicates.ForMethod("CalculatorService", "Add");
        var method = typeof(CalculatorService).GetMethod(nameof(CalculatorService.Add))!;
        Assert.True(predicate(method));

        // Wrong service name → false.
        var predicate2 = Predicates.ForMethod("OtherService", "Add");
        Assert.False(predicate2(method));
    }

    [Fact]
    public void Predicates_Implement_MatchesImplementingType()
    {
        var predicate = Predicates.Implement(typeof(ICalculatorService));
        var method = typeof(CalculatorService).GetMethod(nameof(CalculatorService.Add))!;
        Assert.True(predicate(method));
    }

    // ========================================================================
    // InterceptorCollectionExtensions — AddTyped, AddServiced, AddDelegate
    // ========================================================================

    [Fact]
    public void InterceptorCollection_AddTyped_WithType_Works()
    {
        var collection = new InterceptorCollection();
        collection.AddTyped(typeof(LoggingTestInterceptor), Predicates.ForService("CalculatorService"));

        Assert.Single(collection);
    }

    [Fact]
    public void InterceptorCollection_AddTyped_Generic_Works()
    {
        var collection = new InterceptorCollection();
        collection.AddTyped<LoggingTestInterceptor>(Predicates.ForService("CalculatorService"));

        Assert.Single(collection);
    }

    [Fact]
    public void InterceptorCollection_AddTyped_WithArgs_Works()
    {
        var collection = new InterceptorCollection();
        collection.AddTyped(typeof(LoggingTestInterceptor), new object[] { "test" }, Predicates.ForService("CalculatorService"));

        Assert.Single(collection);
    }

    [Fact]
    public void InterceptorCollection_AddTyped_Generic_WithArgs_Works()
    {
        var collection = new InterceptorCollection();
        collection.AddTyped<LoggingTestInterceptor>(new object[] { "test" }, Predicates.ForService("CalculatorService"));

        Assert.Single(collection);
    }

    [Fact]
    public void InterceptorCollection_AddServiced_WithType_Works()
    {
        var collection = new InterceptorCollection();
        collection.AddServiced(typeof(LoggingTestInterceptor), Predicates.ForService("CalculatorService"));

        Assert.Single(collection);
    }

    [Fact]
    public void InterceptorCollection_AddServiced_Generic_Works()
    {
        var collection = new InterceptorCollection();
        collection.AddServiced<LoggingTestInterceptor>(Predicates.ForService("CalculatorService"));

        Assert.Single(collection);
    }

    [Fact]
    public void InterceptorCollection_AddDelegate_WithOrder_Works()
    {
        var collection = new InterceptorCollection();
        collection.AddDelegate(next => ctx => next(ctx), 5, Predicates.ForService("CalculatorService"));

        Assert.Single(collection);
    }

    [Fact]
    public void InterceptorCollection_AddTyped_NullCollection_Throws()
    {
        InterceptorCollection? nullCollection = null;
        Assert.Throws<ArgumentNullException>(() => nullCollection!.AddTyped(typeof(LoggingTestInterceptor)));
        Assert.Throws<ArgumentNullException>(() => nullCollection!.AddTyped<LoggingTestInterceptor>());
        Assert.Throws<ArgumentNullException>(() => nullCollection!.AddServiced(typeof(LoggingTestInterceptor)));
        Assert.Throws<ArgumentNullException>(() => nullCollection!.AddServiced<LoggingTestInterceptor>());
        Assert.Throws<ArgumentNullException>(() => nullCollection!.AddDelegate(next => ctx => next(ctx)));
    }

    [Fact]
    public void InterceptorCollection_AddTyped_NullType_Throws()
    {
        var collection = new InterceptorCollection();
        Assert.Throws<ArgumentNullException>(() => collection.AddTyped(null!));
    }

    [Fact]
    public void InterceptorCollection_AddTyped_WithArgs_NullPredicates_Throws()
    {
        var collection = new InterceptorCollection();
        Assert.Throws<ArgumentNullException>(() => collection.AddTyped(typeof(LoggingTestInterceptor), new object[] { "test" }, null!));
    }

    // ========================================================================
    // Generic service resolution through ServiceTable
    // ========================================================================

    [Fact]
    public void GenericService_OpenGenericRegistration_ResolvesCorrectly()
    {
        var context = new ServiceContext();
        context.AddType(typeof(IGenericRepository<>), typeof(GenericRepository<>), Lifetime.Singleton);

        var resolver = context.Build();
        var service = resolver.Resolve<IGenericRepository<ConstraintTestClass>>();
        Assert.NotNull(service);
        Assert.IsType<GenericRepository<ConstraintTestClass>>(service);
    }

    [Fact]
    public void GenericService_MultipleImplementations_ResolveMany_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        context.AddDelegate<ITestService>(r => new TestServiceImpl2(), Lifetime.Transient);

        var resolver = context.Build();
        var services = resolver.ResolveMany<ITestService>();
        Assert.Equal(2, services.Count());
    }

    [Fact]
    public void GenericService_IEnumerableResolution_Works()
    {
        var context = new ServiceContext();
        context.AddType<ITestService, TestServiceImpl>(Lifetime.Singleton);
        context.AddDelegate<ITestService>(r => new TestServiceImpl2(), Lifetime.Transient);

        var resolver = context.Build();
        var enumerable = resolver.Resolve<IEnumerable<ITestService>>();
        Assert.NotNull(enumerable);
        Assert.Equal(2, enumerable!.Count());
    }

    [Fact]
    public void GenericService_GenericWithMultipleTypeParams_Works()
    {
        var context = new ServiceContext();
        context.AddType(typeof(IPairService<,>), typeof(PairService<,>), Lifetime.Singleton);

        var resolver = context.Build();
        var service = resolver.Resolve<IPairService<int, string>>();
        Assert.NotNull(service);
        var pair = service!.CreatePair(1, "one");
        Assert.Equal(1, pair.Key);
        Assert.Equal("one", pair.Value);
    }

    // ========================================================================
    // ReflectionUtils — GetMethodBySignature and more scenarios
    // ========================================================================

    [Fact]
    public void ReflectionUtils_GetMethodBySignature_GenericMethod_FindsMatch()
    {
        var typeInfo = typeof(GenericRepository<ConstraintTestClass>).GetTypeInfo();
        var sourceMethod = typeof(IGenericRepository<ConstraintTestClass>).GetMethod(nameof(IGenericRepository<ConstraintTestClass>.GetById))!;

        var found = typeInfo.GetMethodBySignature(sourceMethod);
        Assert.NotNull(found);
        Assert.Equal(nameof(IGenericRepository<ConstraintTestClass>.GetById), found!.Name);
    }

    [Fact]
    public void ReflectionUtils_IsReturnTask_FalseForVoid()
    {
        var method = typeof(IReturnKindService).GetMethod(nameof(IReturnKindService.VoidMethod))!;
        Assert.False(method.IsReturnTask());
    }

    [Fact]
    public void ReflectionUtils_IsReturnValueTask_FalseForVoid()
    {
        var method = typeof(IReturnKindService).GetMethod(nameof(IReturnKindService.VoidMethod))!;
        Assert.False(method.IsReturnValueTask());
    }

    [Fact]
    public void ReflectionUtils_IsVisibleAndVirtual_FalseForPrivateMethod()
    {
        var method = typeof(PrivateMethodClass).GetMethod("PrivateMethod", BindingFlags.NonPublic | BindingFlags.Instance)!;
        Assert.False(method.IsVisibleAndVirtual());
    }

    [Fact]
    public void ReflectionUtils_IsProxyType_Null_Throws()
    {
        TypeInfo? nullTypeInfo = null;
        Assert.Throws<ArgumentNullException>(() => nullTypeInfo!.IsProxyType());
    }

    [Fact]
    public void ReflectionUtils_CanInherited_Null_Throws()
    {
        TypeInfo? nullTypeInfo = null;
        Assert.Throws<ArgumentNullException>(() => nullTypeInfo!.CanInherited());
    }

    [Fact]
    public void ReflectionUtils_IsNonAspect_TypeInfo_Null_Throws()
    {
        TypeInfo? nullTypeInfo = null;
        Assert.Throws<ArgumentNullException>(() => nullTypeInfo!.IsNonAspect());
    }

    [Fact]
    public void ReflectionUtils_IsNonAspect_MethodInfo_Null_Throws()
    {
        MethodInfo? nullMethodInfo = null;
        Assert.Throws<ArgumentNullException>(() => nullMethodInfo!.IsNonAspect());
    }

    [Fact]
    public void ReflectionUtils_IsReturnTask_Null_Throws()
    {
        MethodInfo? nullMethodInfo = null;
        Assert.Throws<ArgumentNullException>(() => nullMethodInfo!.IsReturnTask());
    }

    [Fact]
    public void ReflectionUtils_IsReturnValueTask_Null_Throws()
    {
        MethodInfo? nullMethodInfo = null;
        Assert.Throws<ArgumentNullException>(() => nullMethodInfo!.IsReturnValueTask());
    }

    // ========================================================================
    // Helper types
    // ========================================================================

    public sealed class SealedTestClass { }

    public class ConstraintTestClass { }

    public class LoggingTestInterceptor : AbstractInterceptorAttribute
    {
        private readonly string _label;

        public LoggingTestInterceptor()
        {
            _label = "test";
        }

        public LoggingTestInterceptor(string label)
        {
            _label = label;
        }

        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            InterceptorLog.Entries.Add($"LoggingTest[{_label}].Before");
            await context.Invoke(next);
            InterceptorLog.Entries.Add($"LoggingTest[{_label}].After");
        }
    }

    public interface ITestService
    {
        string GetName();
    }

    public class TestServiceImpl : ITestService
    {
        public string GetName() => "test-impl";
    }

    public class TestServiceImpl2 : ITestService
    {
        public string GetName() => "test-impl-2";
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

    public class PrivateMethodClass
    {
        private void PrivateMethod() { }
        public virtual void PublicVirtualMethod() { }
    }
}
