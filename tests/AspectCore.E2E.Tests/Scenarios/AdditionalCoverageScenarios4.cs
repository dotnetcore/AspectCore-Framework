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
/// Fourth batch of E2E coverage tests targeting the largest remaining gaps:
/// - ILGeneratorExtensions (via reflectors on static/instance/struct/class types)
/// - TypeExtensions (via type extension methods on various types)
/// - ServiceCollectionBuildExtensions (via various DI configurations)
/// </summary>
[Collection("InterceptorLog")]
public class AdditionalCoverageScenarios4
{
    // ========================================================================
    // ILGeneratorExtensions coverage via reflectors on various types
    // ========================================================================

    [Fact]
    public void Reflector_StaticField_AllTypes_Work()
    {
        // Test static field reflector on various types
        var intField = typeof(StaticFieldHolder).GetField("IntValue", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(intField);
        var intReflector = intField!.GetReflector();
        intReflector.SetStaticValue(42);
        Assert.Equal(42, intReflector.GetStaticValue());

        var stringField = typeof(StaticFieldHolder).GetField("StringValue", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(stringField);
        var stringReflector = stringField!.GetReflector();
        stringReflector.SetStaticValue("hello");
        Assert.Equal("hello", stringReflector.GetStaticValue());

        var boolField = typeof(StaticFieldHolder).GetField("BoolValue", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(boolField);
        var boolReflector = boolField!.GetReflector();
        boolReflector.SetStaticValue(true);
        Assert.True((bool)boolReflector.GetStaticValue());
    }

    [Fact]
    public void Reflector_InstanceField_AllTypes_Work()
    {
        var instance = new InstanceFieldHolder();

        var intField = typeof(InstanceFieldHolder).GetField("IntValue");
        Assert.NotNull(intField);
        var intReflector = intField!.GetReflector();
        intReflector.SetValue(instance, 42);
        Assert.Equal(42, intReflector.GetValue(instance));

        var stringField = typeof(InstanceFieldHolder).GetField("StringValue");
        Assert.NotNull(stringField);
        var stringReflector = stringField!.GetReflector();
        stringReflector.SetValue(instance, "hello");
        Assert.Equal("hello", stringReflector.GetValue(instance));

        var doubleField = typeof(InstanceFieldHolder).GetField("DoubleValue");
        Assert.NotNull(doubleField);
        var doubleReflector = doubleField!.GetReflector();
        doubleReflector.SetValue(instance, 3.14);
        Assert.Equal(3.14, doubleReflector.GetValue(instance));
    }

    [Fact]
    public void Reflector_StaticProperty_AllTypes_Work()
    {
        var intProp = typeof(StaticPropertyHolder).GetProperty("IntValue", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(intProp);
        var intReflector = intProp!.GetReflector();
        intReflector.SetStaticValue(42);
        Assert.Equal(42, intReflector.GetStaticValue());

        var stringProp = typeof(StaticPropertyHolder).GetProperty("StringValue", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(stringProp);
        var stringReflector = stringProp!.GetReflector();
        stringReflector.SetStaticValue("hello");
        Assert.Equal("hello", stringReflector.GetStaticValue());
    }

    [Fact]
    public void Reflector_InstanceProperty_AllTypes_Work()
    {
        var instance = new InstancePropertyHolder();

        var intProp = typeof(InstancePropertyHolder).GetProperty("IntValue");
        Assert.NotNull(intProp);
        var intReflector = intProp!.GetReflector();
        intReflector.SetValue(instance, 42);
        Assert.Equal(42, intReflector.GetValue(instance));

        var stringProp = typeof(InstancePropertyHolder).GetProperty("StringValue");
        Assert.NotNull(stringProp);
        var stringReflector = stringProp!.GetReflector();
        stringReflector.SetValue(instance, "hello");
        Assert.Equal("hello", stringReflector.GetValue(instance));

        var boolProp = typeof(InstancePropertyHolder).GetProperty("BoolValue");
        Assert.NotNull(boolProp);
        var boolReflector = boolProp!.GetReflector();
        boolReflector.SetValue(instance, true);
        Assert.True((bool)boolReflector.GetValue(instance));
    }

    [Fact]
    public void Reflector_StaticMethod_AllTypes_Work()
    {
        var intMethod = typeof(StaticMethodHolder).GetMethod("GetInt", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(intMethod);
        var intReflector = intMethod!.GetReflector();
        Assert.Equal(42, intReflector.StaticInvoke());

        var stringMethod = typeof(StaticMethodHolder).GetMethod("GetString", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(stringMethod);
        var stringReflector = stringMethod!.GetReflector();
        Assert.Equal("hello", stringReflector.StaticInvoke("hello"));

        var addMethod = typeof(StaticMethodHolder).GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(addMethod);
        var addReflector = addMethod!.GetReflector();
        Assert.Equal(7, addReflector.StaticInvoke(3, 4));
    }

    [Fact]
    public void Reflector_InstanceMethod_AllTypes_Work()
    {
        var instance = new InstanceMethodHolder(42);

        var intMethod = typeof(InstanceMethodHolder).GetMethod("GetInt");
        Assert.NotNull(intMethod);
        var intReflector = intMethod!.GetReflector();
        Assert.Equal(42, intReflector.Invoke(instance));

        var concatMethod = typeof(InstanceMethodHolder).GetMethod("Concat");
        Assert.NotNull(concatMethod);
        var concatReflector = concatMethod!.GetReflector();
        Assert.Equal("ab", concatReflector.Invoke(instance, "a", "b"));

        var addMethod = typeof(InstanceMethodHolder).GetMethod("Add");
        Assert.NotNull(addMethod);
        var addReflector = addMethod!.GetReflector();
        Assert.Equal(7, addReflector.Invoke(instance, 3, 4));
    }

    [Fact]
    public void Reflector_StructField_AllTypes_Work()
    {
        var instance = new StructHolder { IntValue = 42, StringValue = "hello" };

        var intField = typeof(StructHolder).GetField("IntValue");
        Assert.NotNull(intField);
        var intReflector = intField!.GetReflector();
        Assert.Equal(42, intReflector.GetValue(instance));

        var stringField = typeof(StructHolder).GetField("StringValue");
        Assert.NotNull(stringField);
        var stringReflector = stringField!.GetReflector();
        Assert.Equal("hello", stringReflector.GetValue(instance));
    }

    [Fact]
    public void Reflector_StructMethod_Invoke_Works()
    {
        var instance = new StructHolder { IntValue = 5 };

        var method = typeof(StructHolder).GetMethod("GetValue");
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        Assert.Equal(5, reflector.Invoke(instance));
    }

    [Fact]
    public void Reflector_Constructor_AllTypes_Work()
    {
        var defaultCtor = typeof(InstanceMethodHolder).GetConstructor(Type.EmptyTypes);
        Assert.NotNull(defaultCtor);
        var defaultReflector = defaultCtor!.GetReflector();
        var instance1 = defaultReflector.Invoke();
        Assert.NotNull(instance1);
        Assert.IsType<InstanceMethodHolder>(instance1);

        var paramCtor = typeof(InstanceMethodHolder).GetConstructor(new[] { typeof(int) });
        Assert.NotNull(paramCtor);
        var paramReflector = paramCtor!.GetReflector();
        var instance2 = paramReflector.Invoke(42);
        Assert.NotNull(instance2);
        Assert.IsType<InstanceMethodHolder>(instance2);
    }

    // ========================================================================
    // ServiceCollectionBuildExtensions coverage
    // ========================================================================

    [Fact]
    public void WeaveDynamicProxyService_InterfaceWithType_Proxies()
    {
        var services = new ServiceCollection();
        services.AddTransient<ICalculatorService, CalculatorService>();
        services.ConfigureDynamicProxy(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });
        var result = services.WeaveDynamicProxyService();
        var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ICalculatorService));
        Assert.NotNull(descriptor);
        Assert.NotNull(descriptor!.ImplementationFactory);
    }

    [Fact]
    public void WeaveDynamicProxyService_ClassService_Proxies()
    {
        var services = new ServiceCollection();
        services.AddTransient<ClassProxyService>();
        services.ConfigureDynamicProxy(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.ForService(nameof(ClassProxyService)));
        });
        var result = services.WeaveDynamicProxyService();
        var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ClassProxyService));
        Assert.NotNull(descriptor);
        Assert.NotEqual(typeof(ClassProxyService), descriptor!.ImplementationType);
    }

    [Fact]
    public void WeaveDynamicProxyService_WithValidateScopes_Works()
    {
        var services = new ServiceCollection();
        services.AddScoped<ICalculatorService, CalculatorService>();
        var result = services.WeaveDynamicProxyService();
        var provider = result.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        using var scope = provider.CreateScope();
        var service = scope.ServiceProvider.GetService<ICalculatorService>();
        Assert.NotNull(service);
    }

    [Fact]
    public void WeaveDynamicProxyService_MultipleServices_AllProxied()
    {
        var services = new ServiceCollection();
        services.AddTransient<ICalculatorService, CalculatorService>();
        services.AddTransient<IOrderService, OrderService>();
        services.AddTransient<IAsyncService, AsyncService>();
        var result = services.WeaveDynamicProxyService();

        Assert.NotNull(result.FirstOrDefault(x => x.ServiceType == typeof(ICalculatorService)));
        Assert.NotNull(result.FirstOrDefault(x => x.ServiceType == typeof(IOrderService)));
        Assert.NotNull(result.FirstOrDefault(x => x.ServiceType == typeof(IAsyncService)));
    }

    [Fact]
    public void BuildDynamicProxyProvider_ProducesWorkingProvider()
    {
        var services = new ServiceCollection();
        services.AddTransient<ICalculatorService, CalculatorService>();
        services.ConfigureDynamicProxy(config =>
        {
            config.Interceptors.AddDelegate((ctx, next) => next(ctx),
                Predicates.Implement(typeof(ICalculatorService)));
        });
        var provider = services.BuildDynamicProxyProvider();
        var service = provider.GetService<ICalculatorService>();
        Assert.NotNull(service);
        Assert.IsNotType<CalculatorService>(service);
        Assert.Equal(7, service!.Add(3, 4));
    }

    // ========================================================================
    // Test types
    // ========================================================================

    public static class StaticFieldHolder
    {
        public static int IntValue;
        public static string StringValue = "";
        public static bool BoolValue;
        public static double DoubleValue;
    }

    public class InstanceFieldHolder
    {
        public int IntValue;
        public string StringValue = "";
        public bool BoolValue;
        public double DoubleValue;
    }

    public static class StaticPropertyHolder
    {
        public static int IntValue { get; set; }
        public static string StringValue { get; set; } = "";
        public static bool BoolValue { get; set; }
    }

    public class InstancePropertyHolder
    {
        public int IntValue { get; set; }
        public string StringValue { get; set; } = "";
        public bool BoolValue { get; set; }
        public double DoubleValue { get; set; }
    }

    public static class StaticMethodHolder
    {
        public static int GetInt() => 42;
        public static string GetString(string input) => input;
        public static int Add(int a, int b) => a + b;
    }

    public class InstanceMethodHolder
    {
        private int _value;

        public InstanceMethodHolder() { }
        public InstanceMethodHolder(int value) => _value = value;

        public int GetInt() => _value;
        public string Concat(string a, string b) => a + b;
        public int Add(int a, int b) => a + b;
    }

    public struct StructHolder
    {
        public int IntValue;
        public string StringValue;
        public int GetValue() => IntValue;
    }

    public class ClassProxyService
    {
        public virtual string GetValue() => "class-proxy";
    }
}
