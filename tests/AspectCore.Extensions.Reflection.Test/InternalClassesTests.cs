using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Extensions.Reflection.Test
{
    public class InternalClassesTests
    {
        private static readonly Assembly ReflectionAssembly = typeof(TypeExtensions).Assembly;

        #region TypeInfoUtils Tests

        private static Type GetTypeInfoUtilsType()
        {
            return ReflectionAssembly.GetType("AspectCore.Extensions.Reflection.Internals.TypeInfoUtils");
        }

        private static object InvokeTypeInfoUtils(string methodName, params object[] args)
        {
            var type = GetTypeInfoUtilsType();
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == args.Length);
            Assert.NotNull(method);
            return method.Invoke(null, args);
        }

        [Fact]
        public void TypeInfoUtils_AreEquivalent_Same_Type_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("AreEquivalent", typeof(int).GetTypeInfo(), typeof(int).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_AreEquivalent_Different_Type_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("AreEquivalent", typeof(int).GetTypeInfo(), typeof(long).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsNullableType_Nullable_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsNullableType", typeof(int?).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsNullableType_NonNullable_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsNullableType", typeof(int).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_GetNonNullableType_Nullable_Returns_Underlying()
        {
            var result = (Type)InvokeTypeInfoUtils("GetNonNullableType", typeof(int?).GetTypeInfo());
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void TypeInfoUtils_GetNonNullableType_NonNullable_Returns_Same()
        {
            var result = (Type)InvokeTypeInfoUtils("GetNonNullableType", typeof(int).GetTypeInfo());
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_Int_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(int).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_String_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(string).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsConvertible_Enum_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsConvertible", typeof(TestEnum).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_Byte_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(byte).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_Int_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(int).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_UInt_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(uint).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsUnsigned_Char_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsUnsigned", typeof(char).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsFloatingPoint_Double_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsFloatingPoint", typeof(double).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsFloatingPoint_Float_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsFloatingPoint", typeof(float).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsFloatingPoint_Int_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsFloatingPoint", typeof(int).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_HasReferenceConversion_Upcast_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("HasReferenceConversion", typeof(string).GetTypeInfo(), typeof(object).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_HasReferenceConversion_Downcast_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("HasReferenceConversion", typeof(object).GetTypeInfo(), typeof(string).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_HasReferenceConversion_Interface_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("HasReferenceConversion", typeof(int).GetTypeInfo(), typeof(IComparable).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_HasReferenceConversion_No_Conversion_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("HasReferenceConversion", typeof(int).GetTypeInfo(), typeof(string).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_HasReferenceConversion_Void_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("HasReferenceConversion", typeof(void).GetTypeInfo(), typeof(int).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_HasReferenceConversion_Object_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("HasReferenceConversion", typeof(int).GetTypeInfo(), typeof(object).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsLegalExplicitVariantDelegateConversion_Same_Delegate_Returns_True()
        {
            var result = (bool)InvokeTypeInfoUtils("IsLegalExplicitVariantDelegateConversion",
                typeof(Func<string>).GetTypeInfo(), typeof(Func<string>).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsLegalExplicitVariantDelegateConversion_Covariant_Returns_True()
        {
            // Func<out T> is covariant, so Func<string> -> Func<object> is valid
            var result = (bool)InvokeTypeInfoUtils("IsLegalExplicitVariantDelegateConversion",
                typeof(Func<string>).GetTypeInfo(), typeof(Func<object>).GetTypeInfo());
            Assert.True(result);
        }

        [Fact]
        public void TypeInfoUtils_IsLegalExplicitVariantDelegateConversion_Not_Delegate_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsLegalExplicitVariantDelegateConversion",
                typeof(int).GetTypeInfo(), typeof(int).GetTypeInfo());
            Assert.False(result);
        }

        [Fact]
        public void TypeInfoUtils_IsLegalExplicitVariantDelegateConversion_Non_Generic_Returns_False()
        {
            var result = (bool)InvokeTypeInfoUtils("IsLegalExplicitVariantDelegateConversion",
                typeof(Action).GetTypeInfo(), typeof(Action).GetTypeInfo());
            Assert.False(result);
        }

        #endregion

        #region ReflectorFindUtils Tests

        private static MethodInfo GetFindMemberMethod()
        {
            var type = ReflectionAssembly.GetType("AspectCore.Extensions.Reflection.ReflectorFindUtils");
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(m => m.Name == "FindMember");
            return method.MakeGenericMethod(typeof(FieldReflector), typeof(FieldInfo));
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Null_Name_Throws()
        {
            var method = GetFindMemberMethod();
            var reflectors = Array.Empty<FieldReflector>();
            Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { reflectors, null }));
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Empty_Array_Returns_Null()
        {
            var method = GetFindMemberMethod();
            var reflectors = Array.Empty<FieldReflector>();
            var result = method.Invoke(null, new object[] { reflectors, "test" });
            Assert.Null(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Single_Item_Found()
        {
            var field = typeof(ReflectorTestClass).GetField("PublicField");
            var reflector = field.GetReflector();
            var reflectors = new FieldReflector[] { reflector };

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, "PublicField" });
            Assert.NotNull(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Single_Item_Not_Found()
        {
            var field = typeof(ReflectorTestClass).GetField("PublicField");
            var reflector = field.GetReflector();
            var reflectors = new FieldReflector[] { reflector };

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, "NonExistent" });
            Assert.Null(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Binary_Search_Found()
        {
            var fields = typeof(ReflectorTestClass).GetFields();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, "PublicField" });
            Assert.NotNull(result);
        }

        [Fact]
        public void ReflectorFindUtils_FindMember_Binary_Search_Not_Found()
        {
            var fields = typeof(ReflectorTestClass).GetFields();
            var reflectors = fields.Select(f => f.GetReflector()).OrderBy(r => r.Name).ToArray();

            var method = GetFindMemberMethod();
            var result = method.Invoke(null, new object[] { reflectors, "NonExistent" });
            Assert.Null(result);
        }

        #endregion

        #region InternalExtensions Tests

        private static Type GetInternalExtensionsType()
        {
            return ReflectionAssembly.GetType("AspectCore.Extensions.Reflection.InternalExtensions");
        }

        private static object InvokeInternalExtensions(string methodName, params object[] args)
        {
            var type = GetInternalExtensionsType();
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public)
                .FirstOrDefault(m => m.Name == methodName && m.GetParameters().Length == args.Length);
            Assert.NotNull(method);
            return method.Invoke(null, args);
        }

        [Fact]
        public void InternalExtensions_GetMethodBySign_Works()
        {
            var method = typeof(InternalTestClass).GetMethod("Add");
            var result = (MethodInfo)InvokeInternalExtensions("GetMethodBySign", typeof(InternalTestClass).GetTypeInfo(), method);
            Assert.NotNull(result);
            Assert.Equal("Add", result.Name);
        }

        [Fact]
        public void InternalExtensions_GetMethod_Expression_Works()
        {
            var type = GetInternalExtensionsType();
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(m => m.Name == "GetMethod" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            Expression<Func<InternalTestClass, int, int, int>> expr = (obj, a, b) => obj.Add(a, b);
            var genericMethod = method.MakeGenericMethod(typeof(Func<InternalTestClass, int, int, int>));
            var result = (MethodInfo)genericMethod.Invoke(null, new object[] { expr });
            Assert.Equal("Add", result.Name);
        }

        [Fact]
        public void InternalExtensions_GetMethod_Expression_Null_Throws()
        {
            var type = GetInternalExtensionsType();
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(m => m.Name == "GetMethod" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType.Name.StartsWith("Expression"));

            var genericMethod = method.MakeGenericMethod(typeof(Func<InternalTestClass, int, int, int>));
            Assert.Throws<TargetInvocationException>(() => genericMethod.Invoke(null, new object[] { null }));
        }

        [Fact]
        public void InternalExtensions_GetMethod_String_Works()
        {
            var type = GetInternalExtensionsType();
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(m => m.Name == "GetMethod" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));

            var genericMethod = method.MakeGenericMethod(typeof(InternalTestClass));
            var result = (MethodInfo)genericMethod.Invoke(null, new object[] { "Add" });
            Assert.Equal("Add", result.Name);
        }

        [Fact]
        public void InternalExtensions_GetMethod_String_Null_Throws()
        {
            var type = GetInternalExtensionsType();
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(m => m.Name == "GetMethod" && m.GetParameters().Length == 1 && m.GetParameters()[0].ParameterType == typeof(string));

            var genericMethod = method.MakeGenericMethod(typeof(InternalTestClass));
            Assert.Throws<TargetInvocationException>(() => genericMethod.Invoke(null, new object[] { null }));
        }

        [Fact]
        public void InternalExtensions_IsCallvirt_Class_Returns_False()
        {
            var method = typeof(InternalTestClass).GetMethod("Add");
            var result = (bool)InvokeInternalExtensions("IsCallvirt", method);
            Assert.False(result);
        }

        [Fact]
        public void InternalExtensions_IsCallvirt_Interface_Returns_True()
        {
            var method = typeof(IInternalTest).GetMethod("DoSomething");
            var result = (bool)InvokeInternalExtensions("IsCallvirt", method);
            Assert.True(result);
        }

        [Fact]
        public void InternalExtensions_GetFullName_Interface_Member_Works()
        {
            var method = typeof(IInternalTest).GetMethod("DoSomething");
            var result = (string)InvokeInternalExtensions("GetFullName", method);
            Assert.Equal("IInternalTest.DoSomething", result);
        }

        [Fact]
        public void InternalExtensions_GetFullName_Class_Member_Works()
        {
            var method = typeof(InternalTestClass).GetMethod("Add");
            var result = (string)InvokeInternalExtensions("GetFullName", method);
            Assert.Equal("Add", result);
        }

        [Fact]
        public void InternalExtensions_IsReturnTask_Task_Returns_True()
        {
            var method = typeof(InternalTestClass).GetMethod("TaskMethod");
            var result = (bool)InvokeInternalExtensions("IsReturnTask", method);
            Assert.True(result);
        }

        [Fact]
        public void InternalExtensions_IsReturnTask_Void_Returns_False()
        {
            var method = typeof(InternalTestClass).GetMethod("Add");
            var result = (bool)InvokeInternalExtensions("IsReturnTask", method);
            Assert.False(result);
        }

        [Fact]
        public void InternalExtensions_GetParameterTypes_Works()
        {
            var method = typeof(InternalTestClass).GetMethod("Add");
            var result = (Type[])InvokeInternalExtensions("GetParameterTypes", method);
            Assert.Equal(2, result.Length);
            Assert.Equal(typeof(int), result[0]);
            Assert.Equal(typeof(int), result[1]);
        }

        [Fact]
        public void InternalExtensions_UnWrapArrayType_Null_Throws()
        {
            var type = GetInternalExtensionsType();
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                .First(m => m.Name == "UnWrapArrayType");
            Assert.Throws<TargetInvocationException>(() => method.Invoke(null, new object[] { null }));
        }

        [Fact]
        public void InternalExtensions_UnWrapArrayType_Not_Array_Returns_Same()
        {
            var result = (Type)InvokeInternalExtensions("UnWrapArrayType", typeof(int).GetTypeInfo());
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void InternalExtensions_UnWrapArrayType_Array_Returns_Element_Type()
        {
            var result = (Type)InvokeInternalExtensions("UnWrapArrayType", typeof(int[]).GetTypeInfo());
            Assert.Equal(typeof(int), result);
        }

        #endregion
    }

    public interface IInternalTest
    {
        void DoSomething();
    }

    public class InternalTestClass : IInternalTest
    {
        public int Add(int a, int b) => a + b;
        public void DoSomething() { }
        public Task TaskMethod() => Task.CompletedTask;
    }
}
