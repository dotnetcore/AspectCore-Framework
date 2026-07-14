using System;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ILEmitVisitorCoverageTests : DynamicProxyTestBase
    {
        // ============================================================
        // VisitReflectorDelegationBody with ByRef parameters
        // Lines 370-384 (args array building with ByRef handling)
        // Lines 395-406 (write-back ByRef params after invocation)
        // ============================================================

        [Fact]
        public void ClassProxy_ExplicitInterface_WithRefParameter_HandlesByRef()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExplicitRefServiceImpl>();
            Assert.NotNull(proxy);
            var service = (IRefExplicitService)proxy;
            int value = 10;
            service.RefMethod(ref value);
            Assert.Equal(20, value);
        }

        [Fact]
        public void ClassProxy_ExplicitInterface_WithOutParameter_HandlesByOut()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExplicitRefServiceImpl>();
            Assert.NotNull(proxy);
            var service = (IRefExplicitService)proxy;
            service.OutMethod(out int value);
            Assert.Equal(42, value);
        }

        [Fact]
        public void ClassProxy_ExplicitInterface_WithMultipleRefParameters_HandlesAll()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExplicitRefServiceImpl>();
            Assert.NotNull(proxy);
            var service = (IRefExplicitService)proxy;
            int a = 1, b = 2;
            service.MultipleRefMethod(ref a, ref b);
            Assert.Equal(2, a);
            Assert.Equal(4, b);
        }

        [Fact]
        public void ClassProxy_ExplicitInterface_WithRefAndReturnValue_HandlesBoth()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExplicitRefServiceImpl>();
            Assert.NotNull(proxy);
            var service = (IRefExplicitService)proxy;
            int value = 5;
            var result = service.RefMethodWithReturn(ref value);
            Assert.Equal(10, value);
            Assert.Equal(15, result);
        }

        [Fact]
        public void ClassProxy_ExplicitInterface_WithRefAndOutParameters_HandlesBoth()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExplicitRefServiceImpl>();
            Assert.NotNull(proxy);
            var service = (IRefExplicitService)proxy;
            int refVal = 7;
            service.RefAndOutMethod(ref refVal, out int outVal);
            Assert.Equal(14, refVal);
            Assert.Equal(99, outVal);
        }

        // ============================================================
        // CopyDefaultValueConstant error paths
        // Lines 686-718 (Missing, ArgumentException catch, nullable handling, Convert.ChangeType, re-throw)
        // Line 277 (outer catch in EmitMethodParameters)
        //
        // Key insight: In C# metadata, default values for long/double/float/etc.
        // are stored as int when the literal fits in an int. This causes SetConstant
        // to throw ArgumentException (type mismatch), triggering the catch block.
        // ============================================================

        [Fact]
        public void ClassProxy_MethodWithLongDefault_TriggersConvertChangeType()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetLong();
            Assert.Equal(5L, result);
        }

        [Fact]
        public void ClassProxy_MethodWithDoubleDefault_TriggersConvertChangeType()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetDouble();
            Assert.Equal(5.0, result);
        }

        [Fact]
        public void ClassProxy_MethodWithFloatDefault_TriggersConvertChangeType()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetFloat();
            Assert.Equal(5f, result);
        }

        [Fact]
        public void ClassProxy_MethodWithShortDefault_TriggersConvertChangeType()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetShort();
            Assert.Equal((short)5, result);
        }

        [Fact]
        public void ClassProxy_MethodWithByteDefault_TriggersConvertChangeType()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetByte();
            Assert.Equal((byte)5, result);
        }

        [Fact]
        public void ClassProxy_MethodWithCharDefault_TriggersConvertChangeType()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetChar();
            Assert.Equal('A', result);
        }

        [Fact]
        public void ClassProxy_MethodWithNullableIntNullDefault_TriggersNullDefaultPath()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetNullableIntNull();
            Assert.Null(result);
        }

        [Fact]
        public void ClassProxy_MethodWithNullableIntValueDefault_TriggersNullablePath()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetNullableIntValue();
            Assert.Equal(5, result);
        }

        [Fact]
        public void ClassProxy_MethodWithNullableEnumDefault_TriggersNullableEnumPath()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetNullableEnum();
            Assert.Equal(TestEnum.B, result);
        }

        [Fact]
        public void ClassProxy_MethodWithGuidDefault_TriggersReThrowPath()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetGuid();
            Assert.Equal(Guid.Empty, result);
        }

        [Fact]
        public void ClassProxy_MethodWithTimeSpanDefault_TriggersReThrowPath()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetTimeSpan();
            Assert.Equal(TimeSpan.Zero, result);
        }

        [Fact]
        public void ClassProxy_MethodWithDateTimeDefault_TriggersDateTimePath()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetDateTime();
            Assert.Equal(default, result);
        }

        [Fact]
        public void ClassProxy_MethodWithDecimalDefault_TriggersDecimalPath()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetDecimal();
            Assert.Equal(5m, result);
        }

        [Fact]
        public void ClassProxy_MethodWithNullableLongDefault_TriggersNullableConvert()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            var result = proxy.GetNullableLong();
            Assert.Equal(5L, result);
        }

        [Fact]
        public void ClassProxy_MethodWithMultipleDefaults_AllWork()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ServiceWithMethodDefaults>();
            Assert.NotNull(proxy);
            Assert.Equal(5L, proxy.GetLong());
            Assert.Equal(5.0, proxy.GetDouble());
            Assert.Equal(5f, proxy.GetFloat());
            Assert.Equal((short)5, proxy.GetShort());
            Assert.Equal((byte)5, proxy.GetByte());
            Assert.Equal('A', proxy.GetChar());
            Assert.Null(proxy.GetNullableIntNull());
            Assert.Equal(5, proxy.GetNullableIntValue());
            Assert.Equal(TestEnum.B, proxy.GetNullableEnum());
            Assert.Equal(Guid.Empty, proxy.GetGuid());
            Assert.Equal(TimeSpan.Zero, proxy.GetTimeSpan());
            Assert.Equal(default, proxy.GetDateTime());
            Assert.Equal(5m, proxy.GetDecimal());
        }

        // ============================================================
        // Interface proxy with default values (also triggers EmitMethodParameters)
        // ============================================================

        [Fact]
        public void InterfaceProxy_WithMethodDefaults_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IInterfaceWithDefaults>();
            Assert.NotNull(proxy);
            // Interface proxy without implementation returns stub defaults
            Assert.Equal(0L, proxy.GetLong());
            Assert.Equal(0, proxy.GetInt());
        }

        // ============================================================
        // DefineGenericParameters for type-level generics
        // Lines 595 (SetBaseTypeConstraint), 597 (SetInterfaceConstraints), 599 (SetCustomAttribute)
        // ============================================================

        [Fact]
        public void InterfaceProxy_OpenGenericWithBaseTypeConstraint_GeneratesProxy()
        {
            var proxyType = ProxyTypeGenerator.CreateInterfaceProxyType(typeof(IGenericWithBaseConstraint<>));
            Assert.NotNull(proxyType);
            Assert.True(proxyType.IsGenericTypeDefinition);
        }

        [Fact]
        public void InterfaceProxy_OpenGenericWithInterfaceConstraint_GeneratesProxy()
        {
            var proxyType = ProxyTypeGenerator.CreateInterfaceProxyType(typeof(IGenericWithInterfaceConstraint<>));
            Assert.NotNull(proxyType);
            Assert.True(proxyType.IsGenericTypeDefinition);
        }

        [Fact]
        public void InterfaceProxy_OpenGenericWithBaseAndInterfaceConstraint_GeneratesProxy()
        {
            var proxyType = ProxyTypeGenerator.CreateInterfaceProxyType(typeof(IGenericWithBaseAndInterfaceConstraint<>));
            Assert.NotNull(proxyType);
            Assert.True(proxyType.IsGenericTypeDefinition);
        }

        [Fact]
        public void InterfaceProxy_OpenGenericWithNewConstraint_GeneratesProxy()
        {
            var proxyType = ProxyTypeGenerator.CreateInterfaceProxyType(typeof(IGenericWithNewConstraint<>));
            Assert.NotNull(proxyType);
            Assert.True(proxyType.IsGenericTypeDefinition);
        }

        [Fact]
        public void InterfaceProxy_OpenGenericWithAttributeOnTypeParam_GeneratesProxy()
        {
            var proxyType = ProxyTypeGenerator.CreateInterfaceProxyType(typeof(IGenericWithParamAttribute<>));
            Assert.NotNull(proxyType);
            Assert.True(proxyType.IsGenericTypeDefinition);
        }

        // ============================================================
        // DefineMethodGenericParameters with interface constraints
        // Line 613 (SetInterfaceConstraints for method generic params)
        // ============================================================

        [Fact]
        public void InterfaceProxy_GenericMethodWithInterfaceConstraint_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericMethodWithConstraint>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void InterfaceProxy_GenericMethodWithMultipleConstraints_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IGenericMethodWithMultipleConstraints>();
            Assert.NotNull(proxy);
        }

        // ============================================================
        // BuildCustomAttribute fallback (no named arguments)
        // Line 639 (fallback when NamedArguments is null)
        // ============================================================

        [Fact]
        public void InterfaceProxy_WithAttributes_NoNamedArguments_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateInterfaceProxy<IAttributeService>();
            Assert.NotNull(proxy);
        }

        [Fact]
        public void ClassProxy_WithAttributes_NoNamedArguments_GeneratesProxy()
        {
            var proxy = ProxyGenerator.CreateClassProxy<AttributeService>();
            Assert.NotNull(proxy);
        }

        // ============================================================
        // Interface proxy with explicit implementation + default values
        // Combines multiple uncovered paths
        // ============================================================

        [Fact]
        public void ClassProxy_ExplicitInterfaceWithDefaultValues_HandlesDefaults()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExplicitWithDefaultServiceImpl>();
            Assert.NotNull(proxy);
            var service = (IExplicitWithDefaultService)proxy;
            var result = service.GetValue();
            Assert.Equal(42, result);
        }

        [Fact]
        public void ClassProxy_ExplicitInterfaceWithLongDefault_HandlesDefaults()
        {
            var proxy = ProxyGenerator.CreateClassProxy<ExplicitWithDefaultServiceImpl>();
            Assert.NotNull(proxy);
            var service = (IExplicitWithDefaultService)proxy;
            var result = service.GetLong();
            Assert.Equal(5L, result);
        }

        // ============================================================
        // Helper: ProxyTypeGenerator instance
        // ============================================================

        private static ProxyTypeGenerator ProxyTypeGenerator
        {
            get
            {
                var configuration = new AspectConfiguration();
                var validatorBuilder = new AspectValidatorBuilder(configuration);
                return new ProxyTypeGenerator(validatorBuilder);
            }
        }

        // ============================================================
        // Service type definitions
        // ============================================================

        // --- ReflectorDelegationBody with ByRef ---

        public interface IRefExplicitService
        {
            void RefMethod(ref int value);
            void OutMethod(out int value);
            void MultipleRefMethod(ref int a, ref int b);
            int RefMethodWithReturn(ref int value);
            void RefAndOutMethod(ref int refVal, out int outVal);
        }

        public class ExplicitRefServiceImpl : IRefExplicitService
        {
            void IRefExplicitService.RefMethod(ref int value) => value *= 2;
            void IRefExplicitService.OutMethod(out int value) => value = 42;
            void IRefExplicitService.MultipleRefMethod(ref int a, ref int b) { a *= 2; b *= 2; }
            int IRefExplicitService.RefMethodWithReturn(ref int value) { value *= 2; return value + 5; }
            void IRefExplicitService.RefAndOutMethod(ref int refVal, out int outVal)
            {
                refVal *= 2;
                outVal = 99;
            }
        }

        // --- CopyDefaultValueConstant error paths ---

        public enum TestEnum { A, B, C }

        public class ServiceWithMethodDefaults
        {
            // long default = 5 → stored as int in metadata → SetConstant(5) throws for long
            // → Convert.ChangeType(5, long) succeeds → SetConstant(5L) succeeds
            public virtual long GetLong(long value = 5) => value;

            // double default = 5 → stored as int → SetConstant(5) throws for double
            // → Convert.ChangeType(5, double) succeeds → SetConstant(5.0) succeeds
            public virtual double GetDouble(double value = 5) => value;

            // float default = 5 → stored as int → same path
            public virtual float GetFloat(float value = 5) => value;

            // short default = 5 → stored as int → same path
            public virtual short GetShort(short value = 5) => value;

            // byte default = 5 → stored as int → same path
            public virtual byte GetByte(byte value = 5) => value;

            // char default = 'A' → stored as char → SetConstant might succeed or trigger path
            public virtual char GetChar(char value = 'A') => value;

            // int? default = null → SetConstant(null) for int? might throw
            // → null + nullable → return
            public virtual int? GetNullableIntNull(int? value = null) => value;

            // int? default = 5 → stored as int → SetConstant(5) for int? throws
            // → nullable + IsInstanceOfType(5) → return
            public virtual int? GetNullableIntValue(int? value = 5) => value;

            // TestEnum? default = TestEnum.B → stored as int → SetConstant(1) for TestEnum? throws
            // → nullable + IsEnum → return
            public virtual TestEnum? GetNullableEnum(TestEnum? e = TestEnum.B) => e;

            // Guid default = default → SetConstant(Guid.Empty) throws (Guid not supported)
            // → Convert.ChangeType(Guid.Empty, Guid) succeeds → SetConstant(Guid.Empty) throws again
            // → inner catch → re-throw → caught by outer catch (line 277)
            public virtual Guid GetGuid(Guid g = default) => g;

            // TimeSpan default = default → SetConstant(TimeSpan.Zero) throws (if not supported)
            // → Convert.ChangeType succeeds → SetConstant throws again → re-throw
            public virtual TimeSpan GetTimeSpan(TimeSpan ts = default) => ts;

            // DateTime default = default → TryGetDefaultValue returns DateTime.MinValue (no FormatException)
            // → SetConstant(DateTime.MinValue) might throw → Convert.ChangeType succeeds → SetConstant succeeds
            public virtual DateTime GetDateTime(DateTime dt = default) => dt;

            // decimal default = 5 → stored as 5m (decimal) in metadata → SetConstant(5m) succeeds
            public virtual decimal GetDecimal(decimal value = 5) => value;

            // long? default = 5 → stored as int → SetConstant(5) for long? throws
            // → nullable + not enum + not IsInstanceOfType(int for long?) → Convert.ChangeType path
            public virtual long? GetNullableLong(long? value = 5) => value;
        }

        // --- Interface with method defaults ---

        public interface IInterfaceWithDefaults
        {
            long GetLong(long value = 5);
            int GetInt(int value = 5);
        }

        // --- DefineGenericParameters for type-level generics ---

        public class GenericBase { }
        public interface IGenericConstraint { }

        public interface IGenericWithBaseConstraint<T> where T : GenericBase
        {
            T GetValue();
        }

        public interface IGenericWithInterfaceConstraint<T> where T : IGenericConstraint
        {
            T GetValue();
        }

        public interface IGenericWithBaseAndInterfaceConstraint<T> where T : GenericBase, IGenericConstraint
        {
            T GetValue();
        }

        public interface IGenericWithNewConstraint<T> where T : class, new()
        {
            T Create();
        }

        // Generic parameter with attribute → triggers line 599 (SetCustomAttribute on GenericTypeParameterBuilder)
        [AttributeUsage(AttributeTargets.GenericParameter)]
        public class GenericParamAttribute : Attribute { }

        public interface IGenericWithParamAttribute<[GenericParam] T> where T : class
        {
            T GetValue();
        }

        // --- DefineMethodGenericParameters with interface constraints ---

        public interface IGenericMethodWithConstraint
        {
            T GetValue<T>() where T : IGenericConstraint;
        }

        public interface IGenericMethodWithMultipleConstraints
        {
            T GetValue<T>() where T : class, IGenericConstraint, new();
        }

        // --- BuildCustomAttribute fallback (no named arguments) ---

        [Obsolete("test")]
        public interface IAttributeService
        {
            [Obsolete("method")]
            void DoSomething();
        }

        [Obsolete("test class")]
        public class AttributeService
        {
            [Obsolete("method")]
            public virtual void DoSomething() { }
        }

        // --- Explicit interface with default values ---

        public interface IExplicitWithDefaultService
        {
            int GetValue(int x = 42);
            long GetLong(long x = 5);
        }

        public class ExplicitWithDefaultServiceImpl : IExplicitWithDefaultService
        {
            int IExplicitWithDefaultService.GetValue(int x) => x;
            long IExplicitWithDefaultService.GetLong(long x) => x;
        }
    }
}
