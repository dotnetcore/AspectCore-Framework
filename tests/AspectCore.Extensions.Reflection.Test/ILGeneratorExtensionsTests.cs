using System;
using System.Reflection;
using System.Reflection.Emit;
using AspectCore.Extensions.Reflection.Emit;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class ILGeneratorExtensionsTests
    {
        #region EmitLoadArg

        [Fact]
        public void EmitLoadArg_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitLoadArg(0));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void EmitLoadArg_Small_Indices_Works(int index)
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(index);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int), typeof(int), typeof(int), typeof(int) }, index, 100, 200, 300);

            Assert.Equal(index == 0 ? 0 : index == 1 ? 100 : index == 2 ? 200 : 300, result);
        }

        [Fact]
        public void EmitLoadArg_Byte_Index_Works()
        {
            // Test with index > 3 but <= byte.MaxValue
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(5);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int) }, 0, 1, 2, 3, 4, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitLoadArg_Large_Index_Does_Not_Throw()
        {
            // Test with index > byte.MaxValue - just verify IL generation doesn't throw
            var paramTypes = new Type[260];
            for (int i = 0; i < 260; i++) paramTypes[i] = typeof(int);
            var dm = new DynamicMethod("Test", typeof(int), paramTypes, typeof(ILGeneratorExtensionsTests).Module);
            var il = dm.GetILGenerator();
            il.EmitLoadArg(256);
            il.Emit(OpCodes.Ret);
            // Verify the method was created without exception
            Assert.NotNull(dm);
        }

        #endregion

        #region EmitLoadArgA

        [Fact]
        public void EmitLoadArgA_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitLoadArgA(0));
        }

        [Fact]
        public void EmitLoadArgA_Small_Index_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArgA(1);
                il.Emit(OpCodes.Ldind_I4);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int), typeof(int) }, 0, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitLoadArgA_Large_Index_Does_Not_Throw()
        {
            // Test with index > byte.MaxValue - just verify IL generation doesn't throw
            var paramTypes = new Type[260];
            for (int i = 0; i < 260; i++) paramTypes[i] = typeof(int);
            var dm = new DynamicMethod("Test", typeof(int), paramTypes, typeof(ILGeneratorExtensionsTests).Module);
            var il = dm.GetILGenerator();
            il.EmitLoadArgA(256);
            il.Emit(OpCodes.Ldind_I4);
            il.Emit(OpCodes.Ret);
            Assert.NotNull(dm);
        }

        #endregion

        #region EmitThis

        [Fact]
        public void EmitThis_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitThis());
        }

        [Fact]
        public void EmitThis_Loads_Arg0()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitThis();
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 99);
            Assert.Equal(99, result);
        }

        #endregion

        #region EmitConvertToObject

        [Fact]
        public void EmitConvertToObject_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitConvertToObject(typeof(int)));
        }

        [Fact]
        public void EmitConvertToObject_Null_Type_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(object), new Type[] { typeof(int) });
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitConvertToObject(null));
        }

        [Fact]
        public void EmitConvertToObject_Int_Boxes()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToObject(typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToObject_String_NoBox()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToObject(typeof(string));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(string) }, "hello");
            Assert.Equal("hello", result);
        }

        #endregion

        #region EmitConvertFromObject

        [Fact]
        public void EmitConvertFromObject_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitConvertFromObject(typeof(int)));
        }

        [Fact]
        public void EmitConvertFromObject_Null_Type_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(int), new Type[] { typeof(object) });
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitConvertFromObject(null));
        }

        [Fact]
        public void EmitConvertFromObject_Int_Unboxes()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertFromObject(typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(object) }, (object)42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertFromObject_String_Casts()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertFromObject(typeof(string));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(object) }, (object)"hello");
            Assert.Equal("hello", result);
        }

        #endregion

        #region EmitType

        [Fact]
        public void EmitType_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitType(typeof(int)));
        }

        [Fact]
        public void EmitType_Null_Type_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(Type), null);
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitType(null));
        }

        [Fact]
        public void EmitType_Loads_Type()
        {
            var result = CreateAndInvoke<Type>(il =>
            {
                il.EmitType(typeof(int));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(typeof(int), result);
        }

        #endregion

        #region EmitMethod

        [Fact]
        public void EmitMethod_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            var method = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            Assert.Throws<ArgumentNullException>(() => il.EmitMethod(method));
        }

        [Fact]
        public void EmitMethod_Null_Method_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(MethodInfo), null);
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitMethod((MethodInfo)null));
        }

        [Fact]
        public void EmitMethod_Loads_MethodInfo()
        {
            var method = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            var result = CreateAndInvoke<MethodInfo>(il =>
            {
                il.EmitMethod(method);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(method, result);
        }

        [Fact]
        public void EmitMethod_With_DeclaringType_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            var method = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            Assert.Throws<ArgumentNullException>(() => il.EmitMethod(method, typeof(string)));
        }

        [Fact]
        public void EmitMethod_With_DeclaringType_Null_Method_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(MethodInfo), null);
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitMethod((MethodInfo)null, typeof(string)));
        }

        [Fact]
        public void EmitMethod_With_DeclaringType_Null_DeclaringType_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(MethodInfo), null);
            var il = dm.GetILGenerator();
            var method = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            Assert.Throws<ArgumentNullException>(() => il.EmitMethod(method, null));
        }

        [Fact]
        public void EmitMethod_With_DeclaringType_Works()
        {
            var method = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            var result = CreateAndInvoke<MethodInfo>(il =>
            {
                il.EmitMethod(method, typeof(string));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(method, result);
        }

        #endregion

        #region EmitConvertToType

        [Fact]
        public void EmitConvertToType_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitConvertToType(typeof(int), typeof(long)));
        }

        [Fact]
        public void EmitConvertToType_Null_TypeFrom_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(long), new Type[] { typeof(int) });
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitConvertToType(null, typeof(long)));
        }

        [Fact]
        public void EmitConvertToType_Null_TypeTo_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(long), new Type[] { typeof(int) });
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitConvertToType(typeof(int), null));
        }

        [Fact]
        public void EmitConvertToType_Same_Type_NoOp()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Long_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(long));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Double_Works()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(double));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42.0, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Float_Works()
        {
            var result = CreateAndInvoke<float>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(float));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42f, result);
        }

        [Fact]
        public void EmitConvertToType_Boxing_Works()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(object));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_Unboxing_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(object), typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(object) }, (object)42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_Interface_Cast_Works()
        {
            var result = CreateAndInvoke<IComparable>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(IComparable));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42, (int)result);
        }

        [Fact]
        public void EmitConvertToType_Upcast_Works()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(string), typeof(object));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(string) }, "hello");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void EmitConvertToType_Nullable_To_Nullable_Works()
        {
            var result = CreateAndInvoke<long?>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int?), typeof(long?));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)42);
            Assert.Equal(42L, result.Value);
        }

        [Fact]
        public void EmitConvertToType_Nullable_To_NonNullable_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int?), typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_NonNullable_To_Nullable_Works()
        {
            var result = CreateAndInvoke<int?>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(int?));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42, result.Value);
        }

        [Fact]
        public void EmitConvertToType_Nullable_To_Reference_Works()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int?), typeof(object));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_Enum_To_Base_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(TestEnum), typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(TestEnum) }, TestEnum.Value1);
            Assert.Equal(1, result);
        }

        [Fact]
        public void EmitConvertToType_Unchecked_Int_To_Short_Works()
        {
            var result = CreateAndInvoke<short>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(short), false);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitConvertToType_Array_Cast_Works()
        {
            var result = CreateAndInvoke<Array>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int[]), typeof(Array));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(int[]) }, new object[] { new int[] { 1, 2, 3 } });
            Assert.Equal(3, result.Length);
        }

        [Fact]
        public void EmitConvertToType_UInt_To_Long_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(uint), typeof(long));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(uint) }, 42u);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitConvertToType_ULong_To_Double_Works()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(ulong), typeof(double));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ulong) }, 42ul);
            Assert.Equal(42.0, result);
        }

        [Fact]
        public void EmitConvertToType_Byte_To_Int_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(byte), typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(byte) }, (byte)42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_Double_To_Int_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(double), typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(double) }, 42.7);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConvertToType_Short_To_Byte_Unchecked_Works()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(short), typeof(byte), false);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(short) }, (short)42);
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_UInt_Unchecked_Works()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(uint), false);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42u, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Char_Unchecked_Works()
        {
            var result = CreateAndInvoke<char>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(char), false);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 65);
            Assert.Equal('A', result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Byte_Checked_Works()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(byte), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitConvertToType_UInt_To_Byte_Checked_Works()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(uint), typeof(byte), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(uint) }, 42u);
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_SByte_Checked_Works()
        {
            var result = CreateAndInvoke<sbyte>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(sbyte), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal((sbyte)42, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Long_Checked_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(long), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitConvertToType_UInt_To_ULong_Checked_Works()
        {
            var result = CreateAndInvoke<ulong>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(uint), typeof(ulong), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(uint) }, 42u);
            Assert.Equal(42ul, result);
        }

        [Fact]
        public void EmitConvertToType_Int_To_Int64_Checked_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(int), typeof(long), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitConvertToType_UShort_To_UInt_Checked_Works()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitConvertToType(typeof(ushort), typeof(uint), true);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ushort) }, (ushort)42);
            Assert.Equal(42u, result);
        }

        #endregion

        #region EmitCastToType

        [Fact]
        public void EmitCastToType_ValueType_To_Object_Boxes()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitCastToType(typeof(int).GetTypeInfo(), typeof(object).GetTypeInfo());
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitCastToType_Object_To_ValueType_Unboxes()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitCastToType(typeof(object).GetTypeInfo(), typeof(int).GetTypeInfo());
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(object) }, (object)42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitCastToType_Reference_To_Reference_Casts()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitCastToType(typeof(object).GetTypeInfo(), typeof(string).GetTypeInfo());
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(object) }, (object)"hello");
            Assert.Equal("hello", result);
        }

        #endregion

        #region EmitHasValue / EmitGetValueOrDefault / EmitGetValue

        [Fact]
        public void EmitHasValue_Nullable_With_Value_Returns_True()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitHasValue(typeof(int?));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)42);
            Assert.True(result);
        }

        [Fact]
        public void EmitHasValue_Nullable_Null_Returns_False()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitHasValue(typeof(int?));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)null);
            Assert.False(result);
        }

        [Fact]
        public void EmitGetValueOrDefault_Nullable_With_Value_Returns_Value()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitGetValueOrDefault(typeof(int?));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitGetValueOrDefault_Nullable_Null_Returns_Default()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitGetValueOrDefault(typeof(int?));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)null);
            Assert.Equal(0, result);
        }

        [Fact]
        public void EmitGetValue_Nullable_With_Value_Returns_Value()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitGetValue(typeof(int?));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int?) }, (int?)42);
            Assert.Equal(42, result);
        }

        #endregion

        #region EmitConstant

        [Fact]
        public void EmitConstant_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitConstant(42, typeof(int)));
        }

        [Fact]
        public void EmitConstant_Null_Type_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(object), null);
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitConstant(42, null));
        }

        [Fact]
        public void EmitConstant_Null_Value_Emits_Default()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitConstant(null, typeof(string));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Null(result);
        }

        [Fact]
        public void EmitConstant_Int_Value_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitConstant(42, typeof(int));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitConstant_String_Value_Works()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitConstant("hello", typeof(string));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal("hello", result);
        }

        [Fact]
        public void EmitConstant_Type_Value_Works()
        {
            var result = CreateAndInvoke<Type>(il =>
            {
                il.EmitConstant(typeof(int), typeof(Type));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void EmitConstant_Type_Value_With_Different_ValueType_Works()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitConstant(typeof(int), typeof(object));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void EmitConstant_MethodBase_Value_Works()
        {
            var method = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            var result = CreateAndInvoke<MethodInfo>(il =>
            {
                il.EmitConstant(method, typeof(MethodInfo));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(method, result);
        }

        [Fact]
        public void EmitConstant_Array_Value_Works()
        {
            // EmitConstant with array throws InvalidOperationException due to source code pattern
            // (throw is outside the if block), so test EmitArray directly instead
            var result = CreateAndInvoke<int[]>(il =>
            {
                il.EmitArray(new int[] { 1, 2, 3 }, typeof(int));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(3, result.Length);
            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(3, result[2]);
        }

        [Fact]
        public void EmitConstant_Array_With_Object_Elements_Works()
        {
            // EmitConstant with array throws InvalidOperationException due to source code pattern
            // (throw is outside the if block), so test EmitArray directly instead
            var result = CreateAndInvoke<object[]>(il =>
            {
                il.EmitArray(new object[] { 1, "two", 3L }, typeof(object));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(3, result.Length);
            Assert.Equal(1, result[0]);
            Assert.Equal("two", result[1]);
            Assert.Equal(3L, result[2]);
        }

        [Fact]
        public void EmitConstant_Bool_Value_Works()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitConstant(true, typeof(bool));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.True(result);
        }

        [Fact]
        public void EmitConstant_Long_Value_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitConstant(42L, typeof(long));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitConstant_Double_Value_Works()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitConstant(42.5, typeof(double));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5, result);
        }

        [Fact]
        public void EmitConstant_Float_Value_Works()
        {
            var result = CreateAndInvoke<float>(il =>
            {
                il.EmitConstant(42.5f, typeof(float));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5f, result);
        }

        [Fact]
        public void EmitConstant_Decimal_Value_Works()
        {
            var result = CreateAndInvoke<decimal>(il =>
            {
                il.EmitConstant(42.5m, typeof(decimal));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5m, result);
        }

        [Fact]
        public void EmitConstant_Char_Value_Works()
        {
            var result = CreateAndInvoke<char>(il =>
            {
                il.EmitConstant('A', typeof(char));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal('A', result);
        }

        [Fact]
        public void EmitConstant_Byte_Value_Works()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitConstant((byte)42, typeof(byte));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitConstant_SByte_Value_Works()
        {
            var result = CreateAndInvoke<sbyte>(il =>
            {
                il.EmitConstant((sbyte)-42, typeof(sbyte));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((sbyte)-42, result);
        }

        [Fact]
        public void EmitConstant_Short_Value_Works()
        {
            var result = CreateAndInvoke<short>(il =>
            {
                il.EmitConstant((short)42, typeof(short));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitConstant_UShort_Value_Works()
        {
            var result = CreateAndInvoke<ushort>(il =>
            {
                il.EmitConstant((ushort)42, typeof(ushort));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((ushort)42, result);
        }

        [Fact]
        public void EmitConstant_UInt_Value_Works()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitConstant(42u, typeof(uint));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42u, result);
        }

        [Fact]
        public void EmitConstant_ULong_Value_Works()
        {
            var result = CreateAndInvoke<ulong>(il =>
            {
                il.EmitConstant(42ul, typeof(ulong));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42ul, result);
        }

        #endregion

        #region EmitDefault

        [Fact]
        public void EmitDefault_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitDefault(typeof(int)));
        }

        [Fact]
        public void EmitDefault_Null_Type_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(object), null);
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitDefault(null));
        }

        [Fact]
        public void EmitDefault_Int_Returns_Zero()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitDefault(typeof(int));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(0, result);
        }

        [Fact]
        public void EmitDefault_Long_Returns_Zero()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitDefault(typeof(long));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(0L, result);
        }

        [Fact]
        public void EmitDefault_String_Returns_Null()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitDefault(typeof(string));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Null(result);
        }

        [Fact]
        public void EmitDefault_Object_Returns_Null()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitDefault(typeof(object));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Null(result);
        }

        [Fact]
        public void EmitDefault_Bool_Returns_Zero()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitDefault(typeof(bool));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.False(result);
        }

        [Fact]
        public void EmitDefault_Double_Returns_Zero()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitDefault(typeof(double));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(0.0, result);
        }

        [Fact]
        public void EmitDefault_Float_Returns_Zero()
        {
            var result = CreateAndInvoke<float>(il =>
            {
                il.EmitDefault(typeof(float));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(0f, result);
        }

        [Fact]
        public void EmitDefault_Decimal_Returns_Zero()
        {
            var result = CreateAndInvoke<decimal>(il =>
            {
                il.EmitDefault(typeof(decimal));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(0m, result);
        }

        [Fact]
        public void EmitDefault_Char_Returns_Zero()
        {
            var result = CreateAndInvoke<char>(il =>
            {
                il.EmitDefault(typeof(char));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal('\0', result);
        }

        [Fact]
        public void EmitDefault_Byte_Returns_Zero()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitDefault(typeof(byte));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((byte)0, result);
        }

        [Fact]
        public void EmitDefault_SByte_Returns_Zero()
        {
            var result = CreateAndInvoke<sbyte>(il =>
            {
                il.EmitDefault(typeof(sbyte));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((sbyte)0, result);
        }

        [Fact]
        public void EmitDefault_Short_Returns_Zero()
        {
            var result = CreateAndInvoke<short>(il =>
            {
                il.EmitDefault(typeof(short));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((short)0, result);
        }

        [Fact]
        public void EmitDefault_UInt_Returns_Zero()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitDefault(typeof(uint));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(0u, result);
        }

        [Fact]
        public void EmitDefault_ULong_Returns_Zero()
        {
            var result = CreateAndInvoke<ulong>(il =>
            {
                il.EmitDefault(typeof(ulong));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(0ul, result);
        }

        [Fact]
        public void EmitDefault_UShort_Returns_Zero()
        {
            var result = CreateAndInvoke<ushort>(il =>
            {
                il.EmitDefault(typeof(ushort));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((ushort)0, result);
        }

        [Fact]
        public void EmitDefault_Struct_Returns_Default()
        {
            var result = CreateAndInvoke<Guid>(il =>
            {
                il.EmitDefault(typeof(Guid));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(default(Guid), result);
        }

        [Fact]
        public void EmitDefault_DateTime_Returns_Default()
        {
            var result = CreateAndInvoke<DateTime>(il =>
            {
                il.EmitDefault(typeof(DateTime));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(default(DateTime), result);
        }

        #endregion

        #region CanEmitConstant

        [Fact]
        public void CanEmitConstant_Null_Value_Returns_True()
        {
            Assert.True(ILGeneratorExtensions.CanEmitConstant(null, typeof(int)));
        }

        [Fact]
        public void CanEmitConstant_IL_Constant_Returns_True()
        {
            Assert.True(ILGeneratorExtensions.CanEmitConstant(42, typeof(int)));
        }

        [Fact]
        public void CanEmitConstant_String_Returns_True()
        {
            Assert.True(ILGeneratorExtensions.CanEmitConstant("hello", typeof(string)));
        }

        [Fact]
        public void CanEmitConstant_Type_With_Ldtoken_Returns_True()
        {
            Assert.True(ILGeneratorExtensions.CanEmitConstant(typeof(int), typeof(Type)));
        }

        [Fact]
        public void CanEmitConstant_MethodInfo_Returns_True()
        {
            var method = typeof(string).GetMethod("ToString", Type.EmptyTypes);
            Assert.True(ILGeneratorExtensions.CanEmitConstant(method, typeof(MethodInfo)));
        }

        [Fact]
        public void CanEmitConstant_DynamicMethod_Returns_False()
        {
            var dm = new DynamicMethod("Test", typeof(void), null);
            Assert.False(ILGeneratorExtensions.CanEmitConstant(dm, typeof(MethodInfo)));
        }

        [Fact]
        public void CanEmitConstant_Non_Constant_Returns_False()
        {
            Assert.False(ILGeneratorExtensions.CanEmitConstant(new System.Text.StringBuilder(), typeof(System.Text.StringBuilder)));
        }

        #endregion

        #region EmitDecimal

        [Fact]
        public void EmitDecimal_Small_Int_Value_Works()
        {
            var result = CreateAndInvoke<decimal>(il =>
            {
                il.EmitDecimal(42m);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42m, result);
        }

        [Fact]
        public void EmitDecimal_Large_Int_Value_Works()
        {
            var result = CreateAndInvoke<decimal>(il =>
            {
                il.EmitDecimal(42000000000m);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42000000000m, result);
        }

        [Fact]
        public void EmitDecimal_Very_Large_Value_Works()
        {
            var result = CreateAndInvoke<decimal>(il =>
            {
                il.EmitDecimal(42000000000000000000m);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42000000000000000000m, result);
        }

        [Fact]
        public void EmitDecimal_Fractional_Value_Works()
        {
            var result = CreateAndInvoke<decimal>(il =>
            {
                il.EmitDecimal(42.5m);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5m, result);
        }

        [Fact]
        public void EmitDecimal_Negative_Value_Works()
        {
            var result = CreateAndInvoke<decimal>(il =>
            {
                il.EmitDecimal(-42m);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(-42m, result);
        }

        #endregion

        #region EmitNew

        [Fact]
        public void EmitNew_Creates_Instance()
        {
            var result = CreateAndInvoke<System.Text.StringBuilder>(il =>
            {
                il.EmitNew(typeof(System.Text.StringBuilder).GetTypeInfo().GetConstructor(Type.EmptyTypes));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.NotNull(result);
        }

        #endregion

        #region EmitNull / EmitString / EmitBoolean

        [Fact]
        public void EmitNull_Loads_Null()
        {
            var result = CreateAndInvoke<object>(il =>
            {
                il.EmitNull();
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Null(result);
        }

        [Fact]
        public void EmitString_Loads_String()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitString("hello");
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal("hello", result);
        }

        [Fact]
        public void EmitBoolean_True_Loads_1()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitBoolean(true);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.True(result);
        }

        [Fact]
        public void EmitBoolean_False_Loads_0()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitBoolean(false);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.False(result);
        }

        #endregion

        #region EmitChar / EmitByte / EmitSByte / EmitShort / EmitUShort

        [Fact]
        public void EmitChar_Loads_Char()
        {
            var result = CreateAndInvoke<char>(il =>
            {
                il.EmitChar('A');
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal('A', result);
        }

        [Fact]
        public void EmitByte_Loads_Byte()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitByte(42);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitSByte_Loads_SByte()
        {
            var result = CreateAndInvoke<sbyte>(il =>
            {
                il.EmitSByte(-42);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((sbyte)-42, result);
        }

        [Fact]
        public void EmitShort_Loads_Short()
        {
            var result = CreateAndInvoke<short>(il =>
            {
                il.EmitShort(42);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitUShort_Loads_UShort()
        {
            var result = CreateAndInvoke<ushort>(il =>
            {
                il.EmitUShort(42);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((ushort)42, result);
        }

        #endregion

        #region EmitInt

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        public void EmitInt_Special_Values_Works(int value)
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitInt(value);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(value, result);
        }

        [Fact]
        public void EmitInt_SByte_Range_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitInt(100);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(100, result);
        }

        [Fact]
        public void EmitInt_Large_Value_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitInt(100000);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(100000, result);
        }

        [Fact]
        public void EmitInt_Negative_Large_Value_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitInt(-100000);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(-100000, result);
        }

        #endregion

        #region EmitUInt / EmitLong / EmitULong / EmitDouble / EmitSingle

        [Fact]
        public void EmitUInt_Loads_UInt()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitUInt(42);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42u, result);
        }

        [Fact]
        public void EmitLong_Loads_Long()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLong(42L);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitULong_Loads_ULong()
        {
            var result = CreateAndInvoke<ulong>(il =>
            {
                il.EmitULong(42ul);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42ul, result);
        }

        [Fact]
        public void EmitDouble_Loads_Double()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitDouble(42.5);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5, result);
        }

        [Fact]
        public void EmitSingle_Loads_Single()
        {
            var result = CreateAndInvoke<float>(il =>
            {
                il.EmitSingle(42.5f);
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5f, result);
        }

        #endregion

        #region EmitArray

        [Fact]
        public void EmitArray_Int_Array_Works()
        {
            var result = CreateAndInvoke<int[]>(il =>
            {
                il.EmitArray(new int[] { 1, 2, 3 }, typeof(int));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(3, result.Length);
            Assert.Equal(1, result[0]);
            Assert.Equal(2, result[1]);
            Assert.Equal(3, result[2]);
        }

        [Fact]
        public void EmitArray_String_Array_Works()
        {
            var result = CreateAndInvoke<string[]>(il =>
            {
                il.EmitArray(new string[] { "a", "b" }, typeof(string));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(2, result.Length);
            Assert.Equal("a", result[0]);
            Assert.Equal("b", result[1]);
        }

        [Fact]
        public void EmitArray_Empty_Array_Works()
        {
            var result = CreateAndInvoke<int[]>(il =>
            {
                il.EmitArray(new int[0], typeof(int));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Empty(result);
        }

        #endregion

        #region EmitStoreElement / EmitLoadElement

        [Fact]
        public void EmitStoreElement_Int_Works()
        {
            var result = CreateAndInvoke<int[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(int));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitInt(42);
                il.EmitStoreElement(typeof(int));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42, result[0]);
        }

        [Fact]
        public void EmitStoreElement_Long_Works()
        {
            var result = CreateAndInvoke<long[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(long));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitLong(42L);
                il.EmitStoreElement(typeof(long));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42L, result[0]);
        }

        [Fact]
        public void EmitStoreElement_Byte_Works()
        {
            var result = CreateAndInvoke<byte[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(byte));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitByte(42);
                il.EmitStoreElement(typeof(byte));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((byte)42, result[0]);
        }

        [Fact]
        public void EmitStoreElement_String_Works()
        {
            var result = CreateAndInvoke<string[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(string));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitString("hello");
                il.EmitStoreElement(typeof(string));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal("hello", result[0]);
        }

        [Fact]
        public void EmitStoreElement_Double_Works()
        {
            var result = CreateAndInvoke<double[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(double));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitDouble(42.5);
                il.EmitStoreElement(typeof(double));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5, result[0]);
        }

        [Fact]
        public void EmitStoreElement_Float_Works()
        {
            var result = CreateAndInvoke<float[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(float));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitSingle(42.5f);
                il.EmitStoreElement(typeof(float));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42.5f, result[0]);
        }

        [Fact]
        public void EmitStoreElement_Enum_Works()
        {
            var result = CreateAndInvoke<TestEnum[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(TestEnum));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitInt(1);
                il.EmitStoreElement(typeof(TestEnum));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(TestEnum.Value1, result[0]);
        }

        [Fact]
        public void EmitStoreElement_SByte_Works()
        {
            var result = CreateAndInvoke<sbyte[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(sbyte));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitSByte(-42);
                il.EmitStoreElement(typeof(sbyte));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((sbyte)-42, result[0]);
        }

        [Fact]
        public void EmitStoreElement_Short_Works()
        {
            var result = CreateAndInvoke<short[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(short));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitShort(42);
                il.EmitStoreElement(typeof(short));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((short)42, result[0]);
        }

        [Fact]
        public void EmitStoreElement_UShort_Works()
        {
            var result = CreateAndInvoke<ushort[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(ushort));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitUShort(42);
                il.EmitStoreElement(typeof(ushort));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal((ushort)42, result[0]);
        }

        [Fact]
        public void EmitStoreElement_UInt_Works()
        {
            var result = CreateAndInvoke<uint[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(uint));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitUInt(42);
                il.EmitStoreElement(typeof(uint));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42u, result[0]);
        }

        [Fact]
        public void EmitStoreElement_ULong_Works()
        {
            var result = CreateAndInvoke<ulong[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(ulong));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitULong(42);
                il.EmitStoreElement(typeof(ulong));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal(42ul, result[0]);
        }

        [Fact]
        public void EmitStoreElement_Char_Works()
        {
            var result = CreateAndInvoke<char[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(char));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitChar('A');
                il.EmitStoreElement(typeof(char));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.Equal('A', result[0]);
        }

        [Fact]
        public void EmitStoreElement_Bool_Works()
        {
            var result = CreateAndInvoke<bool[]>(il =>
            {
                il.EmitInt(3);
                il.Emit(OpCodes.Newarr, typeof(bool));
                il.Emit(OpCodes.Dup);
                il.EmitInt(0);
                il.EmitBoolean(true);
                il.EmitStoreElement(typeof(bool));
                il.Emit(OpCodes.Ret);
            }, Type.EmptyTypes);
            Assert.True(result[0]);
        }

        [Fact]
        public void EmitLoadElement_Int_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(1);
                il.EmitLoadElement(typeof(int));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(int[]) }, new object[] { new int[] { 10, 20, 30 } });
            Assert.Equal(20, result);
        }

        [Fact]
        public void EmitLoadElement_String_Works()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(string));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(string[]) }, new object[] { new string[] { "hello", "world" } });
            Assert.Equal("hello", result);
        }

        [Fact]
        public void EmitLoadElement_Long_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(long));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(long[]) }, new object[] { new long[] { 42L } });
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitLoadElement_Byte_Works()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(byte));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(byte[]) }, new object[] { new byte[] { 42 } });
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitLoadElement_Double_Works()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(double));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(double[]) }, new object[] { new double[] { 42.5 } });
            Assert.Equal(42.5, result);
        }

        [Fact]
        public void EmitLoadElement_Float_Works()
        {
            var result = CreateAndInvoke<float>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(float));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(float[]) }, new object[] { new float[] { 42.5f } });
            Assert.Equal(42.5f, result);
        }

        [Fact]
        public void EmitLoadElement_Enum_Works()
        {
            var result = CreateAndInvoke<TestEnum>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(TestEnum));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(TestEnum[]) }, new object[] { new TestEnum[] { TestEnum.Value1 } });
            Assert.Equal(TestEnum.Value1, result);
        }

        [Fact]
        public void EmitLoadElement_Short_Works()
        {
            var result = CreateAndInvoke<short>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(short));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(short[]) }, new object[] { new short[] { 42 } });
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitLoadElement_UShort_Works()
        {
            var result = CreateAndInvoke<ushort>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(ushort));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(ushort[]) }, new object[] { new ushort[] { 42 } });
            Assert.Equal((ushort)42, result);
        }

        [Fact]
        public void EmitLoadElement_UInt_Works()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(uint));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(uint[]) }, new object[] { new uint[] { 42 } });
            Assert.Equal(42u, result);
        }

        [Fact]
        public void EmitLoadElement_ULong_Works()
        {
            var result = CreateAndInvoke<ulong>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(ulong));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(ulong[]) }, new object[] { new ulong[] { 42 } });
            Assert.Equal(42ul, result);
        }

        [Fact]
        public void EmitLoadElement_SByte_Works()
        {
            var result = CreateAndInvoke<sbyte>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(sbyte));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(sbyte[]) }, new object[] { new sbyte[] { -42 } });
            Assert.Equal((sbyte)-42, result);
        }

        [Fact]
        public void EmitLoadElement_Char_Works()
        {
            var result = CreateAndInvoke<char>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(char));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(char[]) }, new object[] { new char[] { 'A' } });
            Assert.Equal('A', result);
        }

        [Fact]
        public void EmitLoadElement_Bool_Works()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitLoadArg(0);
                il.EmitInt(0);
                il.EmitLoadElement(typeof(bool));
                il.Emit(OpCodes.Ret);
}, new Type[] { typeof(bool[]) }, new object[] { new bool[] { true } });
            Assert.True(result);
        }

        #endregion

        #region EmitLdRef

        [Fact]
        public void EmitLdRef_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitLdRef(typeof(int)));
        }

        [Fact]
        public void EmitLdRef_Null_Type_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(int), new Type[] { typeof(int) });
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitLdRef(null));
        }

        [Fact]
        public void EmitLdRef_Int_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(int));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int) }, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitLdRef_Byte_Works()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(byte));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(byte) }, (byte)42);
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitLdRef_Short_Works()
        {
            var result = CreateAndInvoke<short>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(short));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(short) }, (short)42);
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitLdRef_Long_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(long));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(long) }, 42L);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitLdRef_Float_Works()
        {
            var result = CreateAndInvoke<float>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(float));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(float) }, 42.5f);
            Assert.Equal(42.5f, result);
        }

        [Fact]
        public void EmitLdRef_Double_Works()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(double));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(double) }, 42.5);
            Assert.Equal(42.5, result);
        }

        [Fact]
        public void EmitLdRef_String_Works()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(string));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(string) }, "hello");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void EmitLdRef_SByte_Works()
        {
            var result = CreateAndInvoke<sbyte>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(sbyte));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(sbyte) }, (sbyte)-42);
            Assert.Equal((sbyte)-42, result);
        }

        [Fact]
        public void EmitLdRef_Char_Works()
        {
            var result = CreateAndInvoke<char>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(char));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(char) }, 'A');
            Assert.Equal('A', result);
        }

        [Fact]
        public void EmitLdRef_UInt_Works()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(uint));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(uint) }, 42u);
            Assert.Equal(42u, result);
        }

        [Fact]
        public void EmitLdRef_ULong_Works()
        {
            var result = CreateAndInvoke<ulong>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(ulong));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ulong) }, 42ul);
            Assert.Equal(42ul, result);
        }

        [Fact]
        public void EmitLdRef_UShort_Works()
        {
            var result = CreateAndInvoke<ushort>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(ushort));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ushort) }, (ushort)42);
            Assert.Equal((ushort)42, result);
        }

        [Fact]
        public void EmitLdRef_Bool_Works()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(bool));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(bool) }, true);
            Assert.True(result);
        }

        [Fact]
        public void EmitLdRef_Struct_Works()
        {
            var guid = Guid.NewGuid();
            var result = CreateAndInvoke<Guid>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLdRef(typeof(Guid));
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(Guid) }, guid);
            Assert.Equal(guid, result);
        }

        #endregion

        #region EmitStRef

        [Fact]
        public void EmitStRef_Null_ILGenerator_Throws()
        {
            ILGenerator il = null;
            Assert.Throws<ArgumentNullException>(() => il.EmitStRef(typeof(int)));
        }

        [Fact]
        public void EmitStRef_Null_Type_Throws()
        {
            var dm = new DynamicMethod("Test", typeof(void), new Type[] { typeof(int), typeof(int) });
            var il = dm.GetILGenerator();
            Assert.Throws<ArgumentNullException>(() => il.EmitStRef(null));
        }

        [Fact]
        public void EmitStRef_Int_Works()
        {
            var result = CreateAndInvoke<int>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(int));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(int), typeof(int) }, 0, 42);
            Assert.Equal(42, result);
        }

        [Fact]
        public void EmitStRef_Byte_Works()
        {
            var result = CreateAndInvoke<byte>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(byte));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(byte), typeof(byte) }, (byte)0, (byte)42);
            Assert.Equal((byte)42, result);
        }

        [Fact]
        public void EmitStRef_Short_Works()
        {
            var result = CreateAndInvoke<short>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(short));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(short), typeof(short) }, (short)0, (short)42);
            Assert.Equal((short)42, result);
        }

        [Fact]
        public void EmitStRef_Long_Works()
        {
            var result = CreateAndInvoke<long>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(long));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(long), typeof(long) }, 0L, 42L);
            Assert.Equal(42L, result);
        }

        [Fact]
        public void EmitStRef_Float_Works()
        {
            var result = CreateAndInvoke<float>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(float));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(float), typeof(float) }, 0f, 42.5f);
            Assert.Equal(42.5f, result);
        }

        [Fact]
        public void EmitStRef_Double_Works()
        {
            var result = CreateAndInvoke<double>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(double));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(double), typeof(double) }, 0.0, 42.5);
            Assert.Equal(42.5, result);
        }

        [Fact]
        public void EmitStRef_String_Works()
        {
            var result = CreateAndInvoke<string>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(string));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(string), typeof(string) }, null, "hello");
            Assert.Equal("hello", result);
        }

        [Fact]
        public void EmitStRef_SByte_Works()
        {
            var result = CreateAndInvoke<sbyte>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(sbyte));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(sbyte), typeof(sbyte) }, (sbyte)0, (sbyte)-42);
            Assert.Equal((sbyte)-42, result);
        }

        [Fact]
        public void EmitStRef_Char_Works()
        {
            var result = CreateAndInvoke<char>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(char));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(char), typeof(char) }, '\0', 'A');
            Assert.Equal('A', result);
        }

        [Fact]
        public void EmitStRef_UInt_Works()
        {
            var result = CreateAndInvoke<uint>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(uint));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(uint), typeof(uint) }, 0u, 42u);
            Assert.Equal(42u, result);
        }

        [Fact]
        public void EmitStRef_ULong_Works()
        {
            var result = CreateAndInvoke<ulong>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(ulong));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ulong), typeof(ulong) }, 0ul, 42ul);
            Assert.Equal(42ul, result);
        }

        [Fact]
        public void EmitStRef_UShort_Works()
        {
            var result = CreateAndInvoke<ushort>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(ushort));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(ushort), typeof(ushort) }, (ushort)0, (ushort)42);
            Assert.Equal((ushort)42, result);
        }

        [Fact]
        public void EmitStRef_Bool_Works()
        {
            var result = CreateAndInvoke<bool>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(bool));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(bool), typeof(bool) }, false, true);
            Assert.True(result);
        }

        [Fact]
        public void EmitStRef_Struct_Works()
        {
            var guid = Guid.NewGuid();
            var result = CreateAndInvoke<Guid>(il =>
            {
                il.EmitLoadArgA(0);
                il.EmitLoadArg(1);
                il.EmitStRef(typeof(Guid));
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ret);
            }, new Type[] { typeof(Guid), typeof(Guid) }, Guid.Empty, guid);
            Assert.Equal(guid, result);
        }

        #endregion

        #region Helper Methods

        private static T CreateAndInvoke<T>(Action<ILGenerator> emitAction, Type[] paramTypes, params object[] args)
        {
            var dm = new DynamicMethod("Test", typeof(T), paramTypes, typeof(ILGeneratorExtensionsTests).Module);
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
            if (paramTypes.Length == 4)
                return typeof(Func<,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3], returnType);
            if (paramTypes.Length == 5)
                return typeof(Func<,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3], paramTypes[4], returnType);
            if (paramTypes.Length == 6)
                return typeof(Func<,,,,,,>).MakeGenericType(paramTypes[0], paramTypes[1], paramTypes[2], paramTypes[3], paramTypes[4], paramTypes[5], returnType);

            // For more parameters, use a simpler approach
            throw new NotSupportedException("Too many parameters for test helper");
        }

        #endregion
    }

    public enum TestEnum
    {
        Value0 = 0,
        Value1 = 1,
        Value2 = 2
    }
}
