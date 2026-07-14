using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class TypeExtensionsTests
    {
        #region GetMethodBySignature

        [Fact]
        public void GetMethodBySignature_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.GetMethodBySignature(default(MethodSignature)));
        }

        [Fact]
        public void GetMethodBySignature_Finds_Method()
        {
            var typeInfo = typeof(FakeMethodClass).GetTypeInfo();
            var method = typeof(FakeMethodClass).GetMethod("Add");
            var signature = new MethodSignature(method);
            var found = typeInfo.GetMethodBySignature(signature);
            Assert.NotNull(found);
            Assert.Equal("Add", found.Name);
        }

        [Fact]
        public void GetMethodBySignature_Not_Found_Returns_Null()
        {
            var typeInfo = typeof(FakeMethodClass).GetTypeInfo();
            var method = typeof(FakeMethodClass).GetMethod("Add");
            var signature = new MethodSignature(method);
            // Use a type that doesn't have this method
            var found = typeof(string).GetTypeInfo().GetMethodBySignature(signature);
            Assert.Null(found);
        }

        #endregion

        #region GetDeclaredMethodBySignature

        [Fact]
        public void GetDeclaredMethodBySignature_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.GetDeclaredMethodBySignature(default(MethodSignature)));
        }

        [Fact]
        public void GetDeclaredMethodBySignature_Finds_Declared_Method()
        {
            var typeInfo = typeof(FakeMethodClass).GetTypeInfo();
            var method = typeof(FakeMethodClass).GetMethod("Add");
            var signature = new MethodSignature(method);
            var found = typeInfo.GetDeclaredMethodBySignature(signature);
            Assert.NotNull(found);
            Assert.Equal("Add", found.Name);
        }

        [Fact]
        public void GetDeclaredMethodBySignature_Not_Found_Returns_Null()
        {
            var typeInfo = typeof(FakeMethodClass).GetTypeInfo();
            var method = typeof(object).GetMethod("ToString");
            var signature = new MethodSignature(method);
            var found = typeInfo.GetDeclaredMethodBySignature(signature);
            Assert.Null(found);
        }

        #endregion

        #region GetDefaultValue

        [Fact]
        public void GetDefaultValue_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.GetDefaultValue());
        }

        [Fact]
        public void GetDefaultValue_Void_Returns_Null()
        {
            var result = typeof(void).GetTypeInfo().GetDefaultValue();
            Assert.Null(result);
        }

        [Fact]
        public void GetDefaultValue_Int_Returns_Zero()
        {
            var result = typeof(int).GetTypeInfo().GetDefaultValue();
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDefaultValue_Long_Returns_Zero()
        {
            var result = typeof(long).GetTypeInfo().GetDefaultValue();
            // GetDefaultValue returns boxed int 0 for integral types
            Assert.Equal(0, Convert.ToInt32(result));
        }

        [Fact]
        public void GetDefaultValue_String_Returns_Null()
        {
            var result = typeof(string).GetTypeInfo().GetDefaultValue();
            Assert.Null(result);
        }

        [Fact]
        public void GetDefaultValue_Bool_Returns_Zero()
        {
            var result = typeof(bool).GetTypeInfo().GetDefaultValue();
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDefaultValue_Double_Returns_Default()
        {
            var result = typeof(double).GetTypeInfo().GetDefaultValue();
            Assert.Equal(default(double), result);
        }

        [Fact]
        public void GetDefaultValue_Float_Returns_Default()
        {
            var result = typeof(float).GetTypeInfo().GetDefaultValue();
            Assert.Equal(default(float), result);
        }

        [Fact]
        public void GetDefaultValue_Decimal_Returns_Zero()
        {
            var result = typeof(decimal).GetTypeInfo().GetDefaultValue();
            Assert.Equal(new decimal(0), result);
        }

        [Fact]
        public void GetDefaultValue_Reference_Type_Returns_Null()
        {
            var result = typeof(object).GetTypeInfo().GetDefaultValue();
            Assert.Null(result);
        }

        [Fact]
        public void GetDefaultValue_Value_Type_Struct_Returns_Instance()
        {
            var result = typeof(Guid).GetTypeInfo().GetDefaultValue();
            Assert.NotNull(result);
            Assert.IsType<Guid>(result);
        }

        [Fact]
        public void GetDefaultValue_Char_Returns_Zero()
        {
            var result = typeof(char).GetTypeInfo().GetDefaultValue();
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDefaultValue_Byte_Returns_Zero()
        {
            var result = typeof(byte).GetTypeInfo().GetDefaultValue();
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDefaultValue_Short_Returns_Zero()
        {
            var result = typeof(short).GetTypeInfo().GetDefaultValue();
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDefaultValue_UInt_Returns_Zero()
        {
            var result = typeof(uint).GetTypeInfo().GetDefaultValue();
            // GetDefaultValue returns boxed int 0 for integral types
            Assert.Equal(0, Convert.ToInt32(result));
        }

        [Fact]
        public void GetDefaultValue_Nullable_Type_Returns_Null()
        {
            var result = typeof(int?).GetTypeInfo().GetDefaultValue();
            Assert.Null(result);
        }

        [Fact]
        public void GetDefaultValue_Type_Extension_Returns_Same()
        {
            var result = typeof(int).GetDefaultValue();
            Assert.Equal(0, result);
        }

        [Fact]
        public void GetDefaultValue_Null_Type_Returns_Null()
        {
            Type type = null;
            var result = type.GetDefaultValue();
            Assert.Null(result);
        }

        #endregion

        #region IsTask

        [Fact]
        public void IsTask_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsTask());
        }

        [Fact]
        public void IsTask_Task_Returns_True()
        {
            Assert.True(typeof(Task).GetTypeInfo().IsTask());
        }

        [Fact]
        public void IsTask_Task_Of_T_Returns_False()
        {
            Assert.False(typeof(Task<int>).GetTypeInfo().IsTask());
        }

        [Fact]
        public void IsTask_ValueTask_Returns_False()
        {
            Assert.False(typeof(ValueTask).GetTypeInfo().IsTask());
        }

        [Fact]
        public void IsTask_String_Returns_False()
        {
            Assert.False(typeof(string).GetTypeInfo().IsTask());
        }

        #endregion

        #region IsValueTask

        [Fact]
        public void IsValueTask_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsValueTask());
        }

        [Fact]
        public void IsValueTask_ValueTask_Returns_True()
        {
            Assert.True(typeof(ValueTask).GetTypeInfo().IsValueTask());
        }

        [Fact]
        public void IsValueTask_ValueTask_Of_T_Returns_False()
        {
            Assert.False(typeof(ValueTask<int>).GetTypeInfo().IsValueTask());
        }

        [Fact]
        public void IsValueTask_Task_Returns_False()
        {
            Assert.False(typeof(Task).GetTypeInfo().IsValueTask());
        }

        [Fact]
        public void IsValueTask_String_Returns_False()
        {
            Assert.False(typeof(string).GetTypeInfo().IsValueTask());
        }

        #endregion

        #region IsTaskWithResult

        [Fact]
        public void IsTaskWithResult_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsTaskWithResult());
        }

        [Fact]
        public void IsTaskWithResult_Task_Of_T_Returns_True()
        {
            Assert.True(typeof(Task<int>).GetTypeInfo().IsTaskWithResult());
        }

        [Fact]
        public void IsTaskWithResult_Task_Returns_False()
        {
            Assert.False(typeof(Task).GetTypeInfo().IsTaskWithResult());
        }

        [Fact]
        public void IsTaskWithResult_String_Returns_False()
        {
            Assert.False(typeof(string).GetTypeInfo().IsTaskWithResult());
        }

        #endregion

        #region IsValueTaskWithResult

        [Fact]
        public void IsValueTaskWithResult_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsValueTaskWithResult());
        }

        [Fact]
        public void IsValueTaskWithResult_ValueTask_Of_T_Returns_True()
        {
            Assert.True(typeof(ValueTask<int>).GetTypeInfo().IsValueTaskWithResult());
        }

        [Fact]
        public void IsValueTaskWithResult_ValueTask_Returns_False()
        {
            Assert.False(typeof(ValueTask).GetTypeInfo().IsValueTaskWithResult());
        }

        #endregion

        #region IsTaskWithVoidTaskResult

        [Fact]
        public void IsTaskWithVoidTaskResult_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsTaskWithVoidTaskResult());
        }

        [Fact]
        public void IsTaskWithVoidTaskResult_String_Returns_False()
        {
            Assert.False(typeof(string).GetTypeInfo().IsTaskWithVoidTaskResult());
        }

        #endregion

        #region IsNullableType

        [Fact]
        public void IsNullableType_Nullable_Int_Returns_True()
        {
            Assert.True(typeof(int?).IsNullableType());
        }

        [Fact]
        public void IsNullableType_Int_Returns_False()
        {
            Assert.False(typeof(int).IsNullableType());
        }

        [Fact]
        public void IsNullableType_String_Returns_False()
        {
            Assert.False(typeof(string).IsNullableType());
        }

        [Fact]
        public void IsNullableType_Nullable_DateTime_Returns_True()
        {
            Assert.True(typeof(DateTime?).IsNullableType());
        }

        #endregion

        #region IsTupleType

        [Fact]
        public void IsTupleType_Null_Type_Throws()
        {
            Type type = null;
            Assert.Throws<ArgumentNullException>(() => type.IsTupleType());
        }

        [Fact]
        public void IsTupleType_Tuple_Returns_True()
        {
            Assert.True(typeof(Tuple<int, string>).IsTupleType());
        }

        [Fact]
        public void IsTupleType_ValueTuple_Returns_True()
        {
            Assert.True(typeof((int, string)).IsTupleType());
        }

        [Fact]
        public void IsTupleType_String_Returns_False()
        {
            Assert.False(typeof(string).IsTupleType());
        }

        [Fact]
        public void IsTupleType_Int_Returns_False()
        {
            Assert.False(typeof(int).IsTupleType());
        }

        #endregion

        #region IsVisible

        [Fact]
        public void IsVisible_Null_TypeInfo_Throws()
        {
            TypeInfo typeInfo = null;
            Assert.Throws<ArgumentNullException>(() => typeInfo.IsVisible());
        }

        [Fact]
        public void IsVisible_Public_Type_Returns_True()
        {
            Assert.True(typeof(string).GetTypeInfo().IsVisible());
        }

        #endregion
    }

    public class FakeMethodClass
    {
        public int Add(int a, int b)
        {
            return a + b;
        }

        public string Greet(string name)
        {
            return $"Hello {name}";
        }
    }
}
