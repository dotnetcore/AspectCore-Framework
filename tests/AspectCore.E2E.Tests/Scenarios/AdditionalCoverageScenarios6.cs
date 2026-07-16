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
/// Sixth batch of E2E coverage tests - comprehensive reflector testing on
/// many type variations to cover ILGeneratorExtensions code paths.
/// </summary>
public class AdditionalCoverageScenarios6
{
    // ========================================================================
    // Comprehensive field reflector tests - all types, both static and instance
    // ========================================================================

    [Fact]
    public void FieldReflector_AllTypes_Static_SetGet_Comprehensive()
    {
        var type = typeof(ComprehensiveFieldHolder);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Static);

        foreach (var field in fields)
        {
            var reflector = field.GetReflector();
            Assert.NotNull(reflector);

            // Set and get with a value appropriate for the type
            var value = GetDefaultValue(field.FieldType);
            reflector.SetStaticValue(value);
            var retrieved = reflector.GetStaticValue();
            Assert.Equal(value, retrieved);
        }
    }

    [Fact]
    public void FieldReflector_AllTypes_Instance_SetGet_Comprehensive()
    {
        var instance = new ComprehensiveFieldHolder();
        var type = typeof(ComprehensiveFieldHolder);
        var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (var field in fields)
        {
            var reflector = field.GetReflector();
            Assert.NotNull(reflector);

            var value = GetDefaultValue(field.FieldType);
            reflector.SetValue(instance, value);
            var retrieved = reflector.GetValue(instance);
            Assert.Equal(value, retrieved);
        }
    }

    [Fact]
    public void PropertyReflector_AllTypes_Static_SetGet_Comprehensive()
    {
        var type = typeof(ComprehensivePropertyHolder);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Static);

        foreach (var prop in props)
        {
            var reflector = prop.GetReflector();
            Assert.NotNull(reflector);

            var value = GetDefaultValue(prop.PropertyType);
            reflector.SetStaticValue(value);
            var retrieved = reflector.GetStaticValue();
            Assert.Equal(value, retrieved);
        }
    }

    [Fact]
    public void PropertyReflector_AllTypes_Instance_SetGet_Comprehensive()
    {
        var instance = new ComprehensivePropertyHolder();
        var type = typeof(ComprehensivePropertyHolder);
        var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in props)
        {
            var reflector = prop.GetReflector();
            Assert.NotNull(reflector);

            var value = GetDefaultValue(prop.PropertyType);
            reflector.SetValue(instance, value);
            var retrieved = reflector.GetValue(instance);
            Assert.Equal(value, retrieved);
        }
    }

    [Fact]
    public void MethodReflector_AllReturnTypes_Static_Invoke_Comprehensive()
    {
        var type = typeof(ComprehensiveMethodHolder);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => !m.IsSpecialName);

        foreach (var method in methods)
        {
            var reflector = method.GetReflector();
            Assert.NotNull(reflector);

            var parameters = method.GetParameters().Select(p => GetDefaultValue(p.ParameterType)).ToArray();
            var result = reflector.StaticInvoke(parameters);

            // Verify the result is of the correct type
            if (method.ReturnType != typeof(void))
            {
                Assert.NotNull(result);
            }
        }
    }

    [Fact]
    public void MethodReflector_AllReturnTypes_Instance_Invoke_Comprehensive()
    {
        var instance = new ComprehensiveMethodHolder();
        var type = typeof(ComprehensiveMethodHolder);
        var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName && m.DeclaringType == type);

        foreach (var method in methods)
        {
            var reflector = method.GetReflector();
            Assert.NotNull(reflector);

            var parameters = method.GetParameters().Select(p => GetDefaultValue(p.ParameterType)).ToArray();
            var result = reflector.Invoke(instance, parameters);

            if (method.ReturnType != typeof(void))
            {
                Assert.NotNull(result);
            }
        }
    }

    [Fact]
    public void ConstructorReflector_AllTypes_CreateInstance_Comprehensive()
    {
        // Test default constructors
        var defaultCtor = typeof(ComprehensiveMethodHolder).GetConstructor(Type.EmptyTypes);
        Assert.NotNull(defaultCtor);
        var defaultReflector = defaultCtor!.GetReflector();
        var instance1 = defaultReflector.Invoke();
        Assert.NotNull(instance1);

        // Test parameterized constructors
        var paramCtor = typeof(ComprehensiveMethodHolder).GetConstructor(new[] { typeof(int), typeof(string) });
        Assert.NotNull(paramCtor);
        var paramReflector = paramCtor!.GetReflector();
        var instance2 = paramReflector.Invoke(42, "test");
        Assert.NotNull(instance2);
    }

    // ========================================================================
    // TypeExtensions comprehensive tests
    // ========================================================================

    [Fact]
    public void TypeExtensions_IsVisible_True_ForPublicTypes()
    {
        Assert.True(typeof(PublicType).GetTypeInfo().IsVisible());
        Assert.True(typeof(int).GetTypeInfo().IsVisible());
        Assert.True(typeof(string).GetTypeInfo().IsVisible());
    }

    [Fact]
    public void TypeExtensions_CanInherited_VariousTypes()
    {
        Assert.True(typeof(InheritableType).GetTypeInfo().CanInherited());
        Assert.False(typeof(SealedType).GetTypeInfo().CanInherited());
        Assert.False(typeof(int).GetTypeInfo().CanInherited());
        Assert.False(typeof(StructTest).GetTypeInfo().CanInherited());
    }

    [Fact]
    public void TypeExtensions_IsNonAspect_VariousTypes()
    {
        Assert.True(typeof(NonAspectType2).GetTypeInfo().IsNonAspect());
        Assert.False(typeof(ICalculatorService).GetTypeInfo().IsNonAspect());
    }

    // ========================================================================
    // Helper methods and types
    // ========================================================================

    private static object GetDefaultValue(Type type)
    {
        if (type == typeof(int)) return 42;
        if (type == typeof(uint)) return 42u;
        if (type == typeof(long)) return 42L;
        if (type == typeof(ulong)) return 42UL;
        if (type == typeof(short)) return (short)42;
        if (type == typeof(ushort)) return (ushort)42;
        if (type == typeof(byte)) return (byte)42;
        if (type == typeof(sbyte)) return (sbyte)-42;
        if (type == typeof(float)) return 42.0f;
        if (type == typeof(double)) return 42.0;
        if (type == typeof(decimal)) return (decimal)42.0;
        if (type == typeof(char)) return 'A';
        if (type == typeof(bool)) return true;
        if (type == typeof(string)) return "test-string";
        if (type == typeof(IntPtr)) return IntPtr.Zero;
        if (type == typeof(Guid)) return Guid.NewGuid();
        if (type == typeof(DateTime)) return DateTime.Now;
        if (type == typeof(TimeSpan)) return TimeSpan.FromSeconds(42);
        if (type.IsEnum) return Enum.ToObject(type, 1);
        if (type.IsClass) return Activator.CreateInstance(type);
        return Activator.CreateInstance(type);
    }

    public class ComprehensiveFieldHolder
    {
        public static int IntValue;
        public static uint UIntValue;
        public static long LongValue;
        public static ulong ULongValue;
        public static short ShortValue;
        public static ushort UShortValue;
        public static byte ByteValue;
        public static sbyte SByteValue;
        public static float FloatValue;
        public static double DoubleValue;
        public static decimal DecimalValue;
        public static char CharValue;
        public static bool BoolValue;
        public static string StringValue = "";
        public static IntPtr IntPtrValue;
        public static Guid GuidValue;
        public static DateTime DateTimeValue;
        public static TimeSpan TimeSpanValue;

        public int InstanceIntValue;
        public uint InstanceUIntValue;
        public long InstanceLongValue;
        public ulong InstanceULongValue;
        public short InstanceShortValue;
        public ushort InstanceUShortValue;
        public byte InstanceByteValue;
        public sbyte InstanceSByteValue;
        public float InstanceFloatValue;
        public double InstanceDoubleValue;
        public decimal InstanceDecimalValue;
        public char InstanceCharValue;
        public bool InstanceBoolValue;
        public string InstanceStringValue = "";
    }

    public class ComprehensivePropertyHolder
    {
        public static int IntValue { get; set; }
        public static uint UIntValue { get; set; }
        public static long LongValue { get; set; }
        public static ulong ULongValue { get; set; }
        public static short ShortValue { get; set; }
        public static ushort UShortValue { get; set; }
        public static byte ByteValue { get; set; }
        public static sbyte SByteValue { get; set; }
        public static float FloatValue { get; set; }
        public static double DoubleValue { get; set; }
        public static decimal DecimalValue { get; set; }
        public static char CharValue { get; set; }
        public static bool BoolValue { get; set; }
        public static string StringValue { get; set; } = "";

        public int InstanceIntValue { get; set; }
        public uint InstanceUIntValue { get; set; }
        public long InstanceLongValue { get; set; }
        public ulong InstanceULongValue { get; set; }
        public short InstanceShortValue { get; set; }
        public ushort InstanceUShortValue { get; set; }
        public byte InstanceByteValue { get; set; }
        public sbyte InstanceSByteValue { get; set; }
        public float InstanceFloatValue { get; set; }
        public double InstanceDoubleValue { get; set; }
        public decimal InstanceDecimalValue { get; set; }
        public char InstanceCharValue { get; set; }
        public bool InstanceBoolValue { get; set; }
        public string InstanceStringValue { get; set; } = "";
    }

    public class ComprehensiveMethodHolder
    {
        public ComprehensiveMethodHolder() { }
        public ComprehensiveMethodHolder(int intVal, string stringVal) { }

        public static int GetInt() => 42;
        public static uint GetUInt() => 42u;
        public static long GetLong() => 42L;
        public static ulong GetULong() => 42UL;
        public static short GetShort() => 42;
        public static ushort GetUShort() => 42;
        public static byte GetByte() => 42;
        public static sbyte GetSByte() => -42;
        public static float GetFloat() => 42.0f;
        public static double GetDouble() => 42.0;
        public static decimal GetDecimal() => 42m;
        public static char GetChar() => 'A';
        public static bool GetBool() => true;
        public static string GetString() => "test";

        public int EchoInt(int value) => value;
        public uint EchoUInt(uint value) => value;
        public long EchoLong(long value) => value;
        public ulong EchoULong(ulong value) => value;
        public short EchoShort(short value) => value;
        public ushort EchoUShort(ushort value) => value;
        public byte EchoByte(byte value) => value;
        public sbyte EchoSByte(sbyte value) => value;
        public float EchoFloat(float value) => value;
        public double EchoDouble(double value) => value;
        public decimal EchoDecimal(decimal value) => value;
        public char EchoChar(char value) => value;
        public bool EchoBool(bool value) => value;
        public string EchoString(string value) => value;

        public int AddInt(int a, int b) => a + b;
        public string ConcatStrings(string a, string b) => a + b;
        public void VoidMethod() { }
        public void VoidMethodWithParams(int a, string b) { }
    }

    public class PublicType { }
    public class InheritableType { }
    public sealed class SealedType { }

    [NonAspect]
    public class NonAspectType2 { }

    public struct StructTest
    {
        public int Value;
        public int GetValue() => Value;
    }
}
