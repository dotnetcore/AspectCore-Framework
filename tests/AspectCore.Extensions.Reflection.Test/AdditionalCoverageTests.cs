using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class AdditionalCoverageTests
    {
        private static readonly Assembly ReflectionAssembly = typeof(TypeExtensions).Assembly;

        #region ReflectorFindUtils - Additional Binary Search Tests

        [Fact]
        public void ReflectorFindUtils_FindMember_Binary_Search_Middle_Found()
        {
            var fields = typeof(MultiFieldClass).GetFields();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            // Find the middle element
            var middleName = reflectors[reflectors.Length / 2].Name;
            var result = method.Invoke(null, new object[] { reflectors, middleName });
            Assert.NotNull(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Binary_Search_First_Found()
        {
            var fields = typeof(MultiFieldClass).GetFields();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, reflectors[0].Name });
            Assert.NotNull(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Binary_Search_Last_Found()
        {
            var fields = typeof(MultiFieldClass).GetFields();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, reflectors[reflectors.Length - 1].Name });
            Assert.NotNull(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Two_Items_First_Found()
        {
            var fields = typeof(MultiFieldClass).GetFields().Take(2).ToArray();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, reflectors[0].Name });
            Assert.NotNull(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Two_Items_Second_Found()
        {
            var fields = typeof(MultiFieldClass).GetFields().Take(2).ToArray();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, reflectors[1].Name });
            Assert.NotNull(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Binary_Search_Before_First_Not_Found()
        {
            var fields = typeof(MultiFieldClass).GetFields();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, "AAA" });
            Assert.Null(result);
        }

        private static MethodInfo GetFindMemberMethod()
        {
            var type = ReflectionAssembly.GetType("AspectCore.Extensions.Reflection.ReflectorFindUtils");
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(m => m.Name == "FindMember");
            return method.MakeGenericMethod(typeof(FieldReflector), typeof(FieldInfo));
        }

        #endregion

        #region ParameterReflector Tests

        [Fact]
        public void ParameterReflector_GetReflector_Works()
        {
            var method = typeof(MultiFieldClass).GetMethod("MethodWithParams");
            var param = method.GetParameters()[0];
            var reflector = param.GetReflector();
            Assert.NotNull(reflector);
        }

        [Fact]
        public void ParameterReflector_Null_Throws()
        {
            ParameterInfo param = null;
            Assert.Throws<ArgumentNullException>(() => param.GetReflector());
        }

        [Fact]
        public void ParameterReflector_Multiple_Parameters_Works()
        {
            var method = typeof(MultiFieldClass).GetMethod("MethodWithParams");
            var parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                var reflector = param.GetReflector();
                Assert.NotNull(reflector);
            }
        }

        #endregion

        #region Additional TypeInfoUtils Tests

        [Fact]
        public void TypeInfoUtils_IsConvertible_Nullable_Int_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(int?).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_Long_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(long).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_Double_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(double).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_Decimal_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(decimal).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_Char_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(char).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_SByte_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(sbyte).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_UShort_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(ushort).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_Long_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(long).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_ULong_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(ulong).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_UShort_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(ushort).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_SByte_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(sbyte).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsFloatingPoint_Decimal_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsFloatingPoint", typeof(decimal).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsFloatingPoint_Nullable_Double_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsFloatingPoint", typeof(double?).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsNullableType_Nullable_Double_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsNullableType", typeof(double?).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_GetNonNullableType_Nullable_Double_Returns_Double()
        {
            var result = (Type)InvokeTypeInfoUtils("GetNonNullableType", typeof(double?).GetTypeInfo());
            Assert.Equal(typeof(double), result);
        }

        private static object InvokeTypeInfoUtils(string methodName, params object[] args)
        {
            var type = ReflectionAssembly.GetType("AspectCore.Extensions.Reflection.Internals.TypeInfoUtils");
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == args.Length);
            Assert.NotNull(method);
            return method.Invoke(null, args);
        }

        #endregion

        #region Additional ILGeneratorExtensions Edge Case Tests

        [Fact]
        public void EmitInt_Negative_One_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<int>(il =>
            {
                il.EmitInt(-1);
                il.Emit(OpCodes.Ret);
            });
            Assert.Equal(-1, result);
        }

        [Fact]
        public void EmitInt_Zero_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<int>(il =>
            {
                il.EmitInt(0);
                il.Emit(OpCodes.Ret);
            });
            Assert.Equal(0, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_SByte_Checked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<sbyte>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(sbyte), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal((sbyte)42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Short_Checked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<short>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(short), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_UShort_Checked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<ushort>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(ushort), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal((ushort)42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_ULong_Checked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<ulong>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(ulong), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42ul, result);
        }

        [Fact]
        public void EmitConvertToType_UInt_To_Int_Checked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(uint), typeof(int), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(uint) }, 42u);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_UShort_To_Short_Checked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<short>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(ushort), typeof(short), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ushort) }, (ushort)42);
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Double_Unchecked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<double>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(double), false);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42.0, result);
        }

        [Fact]
        public void EmitConvertToType_Long_To_Int_Unchecked_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(long), typeof(int), false);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(long) }, 42L);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_Double_To_Float_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<float>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(double), typeof(float));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(double) }, 42.5);
            Assert.Equal(42.5f, result);
        }

        [Fact]
        public void EmitConvertToType_Float_To_Double_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<double>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(float), typeof(double));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(float) }, 42.5f);
            Assert.Equal(42.5, result);
        }

        [Fact]
        public void EmitConvertToType_UShort_To_UInt_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<uint>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(ushort), typeof(uint));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ushort) }, (ushort)42);
            Assert.Equal(42u, result);
        }

        [Fact]
        public void EmitConvertToType_Byte_To_UShort_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<ushort>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(byte), typeof(ushort));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(byte) }, (byte)42);
            Assert.Equal((ushort)42, result);
        }

        [Fact]
        public void EmitConvertToType_Enum_To_Long_Works()
        {
            var result = ILGeneratorExtensionsTestHelper.CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(TestEnum), typeof(long));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(TestEnum) }, TestEnum.Value2);
            Assert.Equal(2L, result);
        }

        #endregion
    }

    public static class ILGeneratorExtensionsTestHelper
    {
        public static T CreateAndInvoke<T>(Action<ILGenerator> emitAction, Type[] paramTypes = null, params object[] args)
        {
            paramTypes = paramTypes ?? Type.EmptyTypes;
            var dm = new DynamicMethod("Test", typeof(T), paramTypes, typeof(ILGeneratorExtensionsTestHelper).Module);
            var il = dm.GetILGenerator();
            emitAction(il);
            var del = dm.CreateDelegate(GetFuncType(typeof(T), paramTypes));
            return (T)del.DynamicInvoke(args);
        }

        private static Type GetFuncType(Type returnType, Type[] paramTypes)
        {
            if (paramTypes.Length == 0)
                return typeof(Func<>).MakeGenericType(returnType);
            if (paramTypes.Length == 1)
                return typeof(Func<,>).MakeGenericType(paramTypes[0], returnType);
            if (paramTypes.Length == 2)
                return typeof(Func<,,>).MakeGenericType(paramTypes[0], paramTypes[1], returnType);
            if (paramTypes.Length == 3)
                return typeof(Func<,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], returnType);
            throw new NotSupportedException("Too many parameters");
        }
    }

    public class MultiFieldClass
    {
        public int Field01;
        public int Field02;
        public int Field03;
        public int Field04;
        public int Field05;
        public int Field06;
        public int Field07;
        public int Field08;
        public int Field09;
        public int Field10;

        public void MethodWithParams(int a, string b, double c) { }
    }
}
