using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.E2E.Tests.Fixtures;
using AspectCore.Extensions.DependencyInjection;
using AspectCore.Extensions.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.E2E.Tests.Scenarios;

/// <summary>
/// Third batch of E2E coverage tests targeting the largest remaining gaps:
/// - Reflection extensions used directly on service types (Field/Property/Method/Constructor reflectors)
/// - DI extension internals (ServiceResolver, ScopeResolverFactory, ServiceScope)
/// - Type extensions and reflector utilities
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios3
{
    // ========================================================================
    // Reflection extensions - enum fields, static fields, struct fields
    // ========================================================================

    [Fact]
    public void FieldReflector_EnumField_GetStaticValue_Works()
    {
        var field = typeof(TestEnum).GetField(nameof(TestEnum.First));
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        var value = reflector.GetStaticValue();
        Assert.Equal(TestEnum.First, value);
    }

    [Fact]
    public void FieldReflector_EnumField_SecondValue_Works()
    {
        var field = typeof(TestEnum).GetField(nameof(TestEnum.Second));
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        var value = reflector.GetStaticValue();
        Assert.Equal(TestEnum.Second, value);
    }

    [Fact]
    public void FieldReflector_StaticField_SetAndGet_Works()
    {
        var field = typeof(ReflectionTestService2).GetField("StaticField", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        reflector.SetStaticValue("test-value");
        Assert.Equal("test-value", reflector.GetStaticValue());
    }

    [Fact]
    public void FieldReflector_InstanceField_SetAndGet_Works()
    {
        var instance = new ReflectionTestService2();
        var field = typeof(ReflectionTestService2).GetField("InstanceField", BindingFlags.Public | BindingFlags.Instance);
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        reflector.SetValue(instance, "instance-value");
        Assert.Equal("instance-value", reflector.GetValue(instance));
    }

    [Fact]
    public void FieldReflector_StructField_GetValue_Works()
    {
        var instance = new StructTest { Value = 42 };
        var field = typeof(StructTest).GetField("Value");
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        var value = reflector.GetValue(instance);
        Assert.Equal(42, value);
    }

    // ========================================================================
    // PropertyReflector - static, instance, struct
    // ========================================================================

    [Fact]
    public void PropertyReflector_StaticProperty_SetAndGet_Works()
    {
        var prop = typeof(ReflectionTestService2).GetProperty("StaticProperty", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(prop);
        var reflector = prop!.GetReflector();
        reflector.SetStaticValue("static-prop");
        Assert.Equal("static-prop", reflector.GetStaticValue());
    }

    [Fact]
    public void PropertyReflector_InstanceProperty_SetAndGet_Works()
    {
        var instance = new ReflectionTestService2();
        var prop = typeof(ReflectionTestService2).GetProperty("InstanceProperty");
        Assert.NotNull(prop);
        var reflector = prop!.GetReflector();
        reflector.SetValue(instance, "instance-prop");
        Assert.Equal("instance-prop", reflector.GetValue(instance));
    }

    [Fact]
    public void PropertyReflector_StructProperty_GetValue_Works()
    {
        var instance = new StructTest { Value = 99 };
        var prop = typeof(StructTest).GetProperty("ValueProp");
        Assert.NotNull(prop);
        var reflector = prop!.GetReflector();
        var value = reflector.GetValue(instance);
        Assert.Equal(99, value);
    }

    // ========================================================================
    // MethodReflector - static, instance, struct
    // ========================================================================

    [Fact]
    public void MethodReflector_StaticMethod_Invoke_Works()
    {
        var method = typeof(ReflectionTestService2).GetMethod("StaticMethod", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        var result = reflector.StaticInvoke("hello");
        Assert.Equal("hello", result);
    }

    [Fact]
    public void MethodReflector_InstanceMethod_Invoke_Works()
    {
        var instance = new ReflectionTestService2();
        var method = typeof(ReflectionTestService2).GetMethod("InstanceMethod");
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        var result = reflector.Invoke(instance, "test");
        Assert.Equal("test", result);
    }

    [Fact]
    public void MethodReflector_StructMethod_Invoke_Works()
    {
        var instance = new StructTest { Value = 5 };
        var method = typeof(StructTest).GetMethod("GetValue");
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        var result = reflector.Invoke(instance);
        Assert.Equal(5, result);
    }

    [Fact]
    public void MethodReflector_MultipleParameters_Invoke_Works()
    {
        var instance = new ReflectionTestService2();
        var method = typeof(ReflectionTestService2).GetMethod("Add");
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        var result = reflector.Invoke(instance, 3, 4);
        Assert.Equal(7, result);
    }

    // ========================================================================
    // ConstructorReflector - various types
    // ========================================================================

    [Fact]
    public void ConstructorReflector_Default_CreatesInstance()
    {
        var ctor = typeof(ReflectionTestService2).GetConstructor(Type.EmptyTypes);
        Assert.NotNull(ctor);
        var reflector = ctor!.GetReflector();
        var instance = reflector.Invoke();
        Assert.NotNull(instance);
        Assert.IsType<ReflectionTestService2>(instance);
    }

    [Fact]
    public void ConstructorReflector_WithParameter_CreatesInstance()
    {
        var ctor = typeof(ReflectionTestService2).GetConstructor(new[] { typeof(string) });
        Assert.NotNull(ctor);
        var reflector = ctor!.GetReflector();
        var instance = reflector.Invoke("hello");
        Assert.NotNull(instance);
        Assert.IsType<ReflectionTestService2>(instance);
    }

    [Fact]
    public void ConstructorReflector_Struct_CreatesInstance()
    {
        var ctor = typeof(StructTest).GetConstructor(new[] { typeof(int) });
        Assert.NotNull(ctor);
        var reflector = ctor!.GetReflector();
        var instance = reflector.Invoke(42);
        Assert.NotNull(instance);
        Assert.IsType<StructTest>(instance);
    }

    // ========================================================================
    // CustomAttributeReflector - on various types
    // ========================================================================

    [Fact]
    public void CustomAttributeReflector_TypeLevel_Works()
    {
        var attributes = typeof(ReflectionTestService2).GetReflector().GetCustomAttributes();
        Assert.NotNull(attributes);
    }

    [Fact]
    public void CustomAttributeReflector_MethodLevel_Works()
    {
        var method = typeof(ReflectionTestService2).GetMethod("InstanceMethod");
        Assert.NotNull(method);
        var attributes = method!.GetReflector().GetCustomAttributes();
        Assert.NotNull(attributes);
    }

    // ========================================================================
    // TypeReflector and TypeExtensions
    // ========================================================================

    [Fact]
    public void TypeReflector_GetMethods_Works()
    {
        var methods = typeof(ReflectionTestService2).GetMethods();
        Assert.NotNull(methods);
        Assert.NotEmpty(methods);
    }

    [Fact]
    public void TypeReflector_GetProperties_Works()
    {
        var properties = typeof(ReflectionTestService2).GetProperties();
        Assert.NotNull(properties);
    }

    // ========================================================================
    // DI extension internals - ServiceResolver, ScopeResolverFactory
    // ========================================================================

    [Fact]
    public void DiServiceResolver_Resolve_ReturnsProxiedService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Singleton);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var resolver = provider.GetService<IServiceResolver>();
        Assert.NotNull(resolver);

        var service = resolver!.Resolve(typeof(ICalculatorService));
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ICalculatorService>(service);
    }

    [Fact]
    public void DiServiceResolver_GetService_ReturnsProxiedService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Singleton);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var resolver = provider.GetService<IServiceResolver>();
        Assert.NotNull(resolver);

        var service = resolver!.GetService(typeof(ICalculatorService));
        Assert.NotNull(service);
        Assert.IsAssignableFrom<ICalculatorService>(service);
    }

    [Fact]
    public void DiScopeResolverFactory_CreateScope_ResolvesService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        var scopeFactory = provider.GetService<IScopeResolverFactory>();
        Assert.NotNull(scopeFactory);

        using (var scope = scopeFactory!.CreateScope())
        {
            var service = scope.Resolve(typeof(ICalculatorService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<ICalculatorService>(service);
        }
    }

    [Fact]
    public void DiServiceScope_FromCreateScope_ResolvesService()
    {
        using var host = new TestHost();
        host.Add<ICalculatorService, CalculatorService>(ServiceLifetime.Scoped);

        var provider = host.CreateServiceProvider(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });

        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<ICalculatorService>();
        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(20, service!.Add(10, 10));
    }

    // ========================================================================
    // Test types
    // ========================================================================

    public enum TestEnum
    {
        First = 1,
        Second = 2
    }

    public struct StructTest
    {
        public int Value;
        public int ValueProp => Value;
        public int GetValue() => Value;

        public StructTest(int value)
        {
            Value = value;
        }
    }

    public class ReflectionTestService2
    {
        public static string StaticField = "initial";
        public string InstanceField = "initial-instance";

        public static string StaticProperty { get; set; } = "initial-prop";
        public string InstanceProperty { get; set; } = "initial-instance-prop";

        public ReflectionTestService2() { }

        public ReflectionTestService2(string value)
        {
            InstanceField = value;
        }

        public static string StaticMethod(string input) => input;

        public string InstanceMethod(string input) => input;

        public int Add(int a, int b) => a + b;
    }
}
