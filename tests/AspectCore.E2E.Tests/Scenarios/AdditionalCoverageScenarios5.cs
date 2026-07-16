using System;
using System.Linq;
using System.Reflection;
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
/// Fifth batch of E2E coverage tests targeting ILGeneratorExtensions and
/// TypeExtensions by exercising reflectors on many different type variations.
/// </summary>
public class AdditionalCoverageScenarios5
{
    // ========================================================================
    // ILGeneratorExtensions coverage - test reflectors on all primitive types
    // ========================================================================

    [Theory]
    [InlineData(typeof(byte), (byte)1)]
    [InlineData(typeof(sbyte), (sbyte)-1)]
    [InlineData(typeof(short), (short)-1)]
    [InlineData(typeof(ushort), (ushort)1)]
    [InlineData(typeof(int), -1)]
    [InlineData(typeof(uint), 1u)]
    [InlineData(typeof(long), -1L)]
    [InlineData(typeof(ulong), 1UL)]
    [InlineData(typeof(float), 1.5f)]
    [InlineData(typeof(double), 1.5)]
    [InlineData(typeof(char), 'a')]
    [InlineData(typeof(bool), true)]
    public void FieldReflector_AllPrimitiveTypes_StaticField_Works(Type type, object value)
    {
        var field = typeof(StaticFieldHolder2).GetField($"{type.Name}Value", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        reflector.SetStaticValue(value);
        Assert.Equal(value, reflector.GetStaticValue());
    }

    [Fact]
    public void FieldReflector_DecimalType_StaticField_Works()
    {
        var field = typeof(StaticFieldHolder2).GetField("DecimalValue", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        reflector.SetStaticValue((decimal)1.5);
        Assert.Equal((decimal)1.5, reflector.GetStaticValue());
    }

    [Theory]
    [InlineData(typeof(byte), (byte)1)]
    [InlineData(typeof(sbyte), (sbyte)-1)]
    [InlineData(typeof(short), (short)-1)]
    [InlineData(typeof(ushort), (ushort)1)]
    [InlineData(typeof(int), -1)]
    [InlineData(typeof(uint), 1u)]
    [InlineData(typeof(long), -1L)]
    [InlineData(typeof(ulong), 1UL)]
    [InlineData(typeof(float), 1.5f)]
    [InlineData(typeof(double), 1.5)]
    [InlineData(typeof(char), 'a')]
    [InlineData(typeof(bool), true)]
    public void FieldReflector_AllPrimitiveTypes_InstanceField_Works(Type type, object value)
    {
        var instance = new InstanceFieldHolder2();
        var field = typeof(InstanceFieldHolder2).GetField($"{type.Name}Value");
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        reflector.SetValue(instance, value);
        Assert.Equal(value, reflector.GetValue(instance));
    }

    [Fact]
    public void FieldReflector_DecimalType_InstanceField_Works()
    {
        var instance = new InstanceFieldHolder2();
        var field = typeof(InstanceFieldHolder2).GetField("DecimalValue");
        Assert.NotNull(field);
        var reflector = field!.GetReflector();
        reflector.SetValue(instance, (decimal)1.5);
        Assert.Equal((decimal)1.5, reflector.GetValue(instance));
    }

    [Theory]
    [InlineData(typeof(byte), (byte)1)]
    [InlineData(typeof(sbyte), (sbyte)-1)]
    [InlineData(typeof(short), (short)-1)]
    [InlineData(typeof(ushort), (ushort)1)]
    [InlineData(typeof(int), -1)]
    [InlineData(typeof(uint), 1u)]
    [InlineData(typeof(long), -1L)]
    [InlineData(typeof(ulong), 1UL)]
    [InlineData(typeof(float), 1.5f)]
    [InlineData(typeof(double), 1.5)]
    [InlineData(typeof(char), 'a')]
    [InlineData(typeof(bool), true)]
    public void PropertyReflector_AllPrimitiveTypes_StaticProperty_Works(Type type, object value)
    {
        var prop = typeof(StaticPropertyHolder2).GetProperty($"{type.Name}Value", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(prop);
        var reflector = prop!.GetReflector();
        reflector.SetStaticValue(value);
        Assert.Equal(value, reflector.GetStaticValue());
    }

    [Fact]
    public void PropertyReflector_DecimalType_StaticProperty_Works()
    {
        var prop = typeof(StaticPropertyHolder2).GetProperty("DecimalValue", BindingFlags.Public | BindingFlags.Static);
        Assert.NotNull(prop);
        var reflector = prop!.GetReflector();
        reflector.SetStaticValue((decimal)1.5);
        Assert.Equal((decimal)1.5, reflector.GetStaticValue());
    }

    [Theory]
    [InlineData(typeof(byte), (byte)1)]
    [InlineData(typeof(sbyte), (sbyte)-1)]
    [InlineData(typeof(short), (short)-1)]
    [InlineData(typeof(ushort), (ushort)1)]
    [InlineData(typeof(int), -1)]
    [InlineData(typeof(uint), 1u)]
    [InlineData(typeof(long), -1L)]
    [InlineData(typeof(ulong), 1UL)]
    [InlineData(typeof(float), 1.5f)]
    [InlineData(typeof(double), 1.5)]
    [InlineData(typeof(char), 'a')]
    [InlineData(typeof(bool), true)]
    public void PropertyReflector_AllPrimitiveTypes_InstanceProperty_Works(Type type, object value)
    {
        var instance = new InstancePropertyHolder2();
        var prop = typeof(InstancePropertyHolder2).GetProperty($"{type.Name}Value");
        Assert.NotNull(prop);
        var reflector = prop!.GetReflector();
        reflector.SetValue(instance, value);
        Assert.Equal(value, reflector.GetValue(instance));
    }

    [Fact]
    public void PropertyReflector_DecimalType_InstanceProperty_Works()
    {
        var instance = new InstancePropertyHolder2();
        var prop = typeof(InstancePropertyHolder2).GetProperty("DecimalValue");
        Assert.NotNull(prop);
        var reflector = prop!.GetReflector();
        reflector.SetValue(instance, (decimal)1.5);
        Assert.Equal((decimal)1.5, reflector.GetValue(instance));
    }

    // ========================================================================
    // MethodReflector - test methods with various return types and parameters
    // ========================================================================

    [Fact]
    public void MethodReflector_MethodWithAllParameterTypes_Works()
    {
        var instance = new MethodHolder2();

        // Test method with multiple parameter types
        var method = typeof(MethodHolder2).GetMethod("ProcessAll");
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        var result = reflector.Invoke(instance, (byte)1, (short)2, 3, 4L, 5.5f, 6.6, 'g', true, "str");
        Assert.Equal("1-2-3-4-5.5-6.6-g-True-str", result);
    }

    [Fact]
    public void MethodReflector_MethodWithRefParameters_Works()
    {
        var instance = new MethodHolder2();
        var method = typeof(MethodHolder2).GetMethod("ModifyRef");
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        var args = new object[] { 5 };
        var result = reflector.Invoke(instance, args);
        Assert.Equal(10, args[0]);
    }

    [Fact]
    public void MethodReflector_MethodWithOutParameters_Works()
    {
        var instance = new MethodHolder2();
        var method = typeof(MethodHolder2).GetMethod("ModifyOut");
        Assert.NotNull(method);
        var reflector = method!.GetReflector();
        var args = new object[] { null };
        var result = reflector.Invoke(instance, args);
        Assert.Equal("output", args[0]);
    }

    // ========================================================================
    // TypeExtensions coverage
    // ========================================================================

    [Fact]
    public void TypeExtensions_IsVisible_True_ForPublicClass()
    {
        Assert.True(typeof(PublicType).GetTypeInfo().IsVisible());
    }

    [Fact]
    public void TypeExtensions_CanInherited_True_ForInheritableClass()
    {
        Assert.True(typeof(InheritableType).GetTypeInfo().CanInherited());
    }

    [Fact]
    public void TypeExtensions_CanInherited_False_ForSealedClass()
    {
        Assert.False(typeof(SealedType).GetTypeInfo().CanInherited());
    }

    [Fact]
    public void TypeExtensions_IsNonAspect_True_ForNonAspectType()
    {
        Assert.True(typeof(NonAspectType2).GetTypeInfo().IsNonAspect());
    }

    [Fact]
    public void TypeExtensions_IsNonAspect_False_ForServiceWithAspects()
    {
        Assert.False(typeof(ICalculatorService).GetTypeInfo().IsNonAspect());
    }

    [Fact]
    public void TypeExtensions_GetMethods_IncludesAllMethods()
    {
        var methods = typeof(MethodHolder2).GetMethods();
        Assert.NotNull(methods);
        Assert.NotEmpty(methods);
    }

    [Fact]
    public void TypeExtensions_GetProperties_IncludesAllProperties()
    {
        var properties = typeof(InstancePropertyHolder2).GetProperties();
        Assert.NotNull(properties);
        Assert.NotEmpty(properties);
    }

    // ========================================================================
    // CustomAttributeExtensions coverage
    // ========================================================================

    [Fact]
    public void CustomAttributeExtensions_GetCustomAttributes_FromMethod_Works()
    {
        var method = typeof(AttributeHolder2).GetMethod("MethodWithAttribute");
        Assert.NotNull(method);
        var attributes = method!.GetReflector().GetCustomAttributes();
        Assert.NotNull(attributes);
        Assert.NotEmpty(attributes);
    }

    [Fact]
    public void CustomAttributeExtensions_GetCustomAttributes_FromType_Works()
    {
        var attributes = typeof(AttributeHolder2).GetReflector().GetCustomAttributes();
        Assert.NotNull(attributes);
    }

    [Fact]
    public void CustomAttributeExtensions_GetCustomAttributes_FromProperty_Works()
    {
        var prop = typeof(AttributeHolder2).GetProperty("PropertyWithAttribute");
        Assert.NotNull(prop);
        var attributes = prop!.GetReflector().GetCustomAttributes();
        Assert.NotNull(attributes);
    }

    // ========================================================================
    // Test types
    // ========================================================================

    public static class StaticFieldHolder2
    {
        public static byte ByteValue;
        public static sbyte SByteValue;
        public static short Int16Value;
        public static ushort UInt16Value;
        public static int Int32Value;
        public static uint UInt32Value;
        public static long Int64Value;
        public static ulong UInt64Value;
        public static float SingleValue;
        public static double DoubleValue;
        public static decimal DecimalValue;
        public static char CharValue;
        public static bool BooleanValue;
    }

    public class InstanceFieldHolder2
    {
        public byte ByteValue;
        public sbyte SByteValue;
        public short Int16Value;
        public ushort UInt16Value;
        public int Int32Value;
        public uint UInt32Value;
        public long Int64Value;
        public ulong UInt64Value;
        public float SingleValue;
        public double DoubleValue;
        public decimal DecimalValue;
        public char CharValue;
        public bool BooleanValue;
    }

    public static class StaticPropertyHolder2
    {
        public static byte ByteValue { get; set; }
        public static sbyte SByteValue { get; set; }
        public static short Int16Value { get; set; }
        public static ushort UInt16Value { get; set; }
        public static int Int32Value { get; set; }
        public static uint UInt32Value { get; set; }
        public static long Int64Value { get; set; }
        public static ulong UInt64Value { get; set; }
        public static float SingleValue { get; set; }
        public static double DoubleValue { get; set; }
        public static decimal DecimalValue { get; set; }
        public static char CharValue { get; set; }
        public static bool BooleanValue { get; set; }
    }

    public class InstancePropertyHolder2
    {
        public byte ByteValue { get; set; }
        public sbyte SByteValue { get; set; }
        public short Int16Value { get; set; }
        public ushort UInt16Value { get; set; }
        public int Int32Value { get; set; }
        public uint UInt32Value { get; set; }
        public long Int64Value { get; set; }
        public ulong UInt64Value { get; set; }
        public float SingleValue { get; set; }
        public double DoubleValue { get; set; }
        public decimal DecimalValue { get; set; }
        public char CharValue { get; set; }
        public bool BooleanValue { get; set; }
    }

    public class MethodHolder2
    {
        public string ProcessAll(byte b, short s, int i, long l, float f, double d, char c, bool b2, string str)
            => $"{b}-{s}-{i}-{l}-{f}-{d}-{c}-{b2}-{str}";

        public void ModifyRef(ref int value) => value *= 2;

        public void ModifyOut(out string value) => value = "output";
    }

    public class PublicType { }
    public class InheritableType { }
    public sealed class SealedType { }

    [NonAspect]
    public class NonAspectType2 { }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Property, AllowMultiple = true)]
    public class TestCoverageAttribute : Attribute { }

    [TestCoverage]
    public class AttributeHolder2
    {
        [TestCoverage]
        public string MethodWithAttribute() => "test";

        [TestCoverage]
        public string PropertyWithAttribute { get; set; } = "";
    }
}
