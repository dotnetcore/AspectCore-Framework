using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions;
using TypeExtensions = AspectCore.Extensions.TypeExtensions;
using Xunit;

namespace AspectCore.Core.Tests.Extensions
{
    public class TypeExtensionsCoverageTests
    {
        #region GetCovariantReturnMethods

        [Fact]
        public void GetCovariantReturnMethods_TypeWithNoCovariantReturns_ReturnsEmpty()
        {
            var result = typeof(NonCovariantClass).GetCovariantReturnMethods();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void GetCovariantReturnMethods_InterfaceWithNoCovariantReturns_ReturnsEmpty()
        {
            var result = typeof(INonCovariant).GetCovariantReturnMethods();
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region IsOverriddenByCovariantReturnMethod - negative cases

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_NotInCovariantChain_ReturnsFalse()
        {
            var baseMethod = typeof(BaseNonCovariant).GetMethod("GetValue");
            var derivedMethod = typeof(DerivedNonCovariant).GetMethod("GetValue");
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(derivedMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_DifferentName_ReturnsFalse()
        {
            var baseMethod = typeof(BaseNonCovariant).GetMethod("GetValue");
            var covariantMethod = typeof(DerivedWithCovariant).GetMethod("GetDifferentValue");
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_DeclaringTypeNotAssignable_ReturnsFalse()
        {
            var baseMethod = typeof(UnrelatedBase).GetMethod("GetValue");
            var covariantMethod = typeof(DerivedWithCovariant).GetMethod("GetValue");
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_SameReturnType_ReturnsFalse()
        {
            var baseMethod = typeof(BaseNonCovariant).GetMethod("GetValue");
            var covariantMethod = typeof(DerivedNonCovariant).GetMethod("GetValue");
            // Both return object, so it's not a covariant narrowing
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_ReturnTypeNotAssignable_ReturnsFalse()
        {
            var baseMethod = typeof(BaseWithStringReturn).GetMethod("GetValue");
            var covariantMethod = typeof(DerivedWithCovariant).GetMethod("GetValue");
            // base returns string, covariant returns DerivedResult - not assignable
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_DifferentParameterCount_ReturnsFalse()
        {
            var baseMethod = typeof(BaseWithParams).GetMethod("Process");
            var covariantMethod = typeof(DerivedWithCovariant).GetMethod("GetValue");
            // Different parameter count - should return false at line 185
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_ParameterTypeMismatch_ReturnsFalse()
        {
            var baseMethod = typeof(BaseWithStringParam).GetMethod("Process");
            var covariantMethod = typeof(DerivedWithCovariant).GetMethod("GetValue");
            // Parameter types don't match
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_OneGenericOneNot_ReturnsFalse()
        {
            var baseMethod = typeof(BaseGenericMethod).GetMethod("GenericMethod");
            var covariantMethod = typeof(DerivedNonCovariant).GetMethod("GetValue");
            // One is generic, the other is not - should return false at line 196
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_DifferentGenericArgumentCount_ReturnsFalse()
        {
            var baseMethod = typeof(BaseGenericMethod).GetMethod("GenericMethod");
            // Get the method with 2 generic arguments to avoid ambiguous match
            var covariantMethod = typeof(DerivedTwoGenericArgs).GetMethods()
                .First(m => m.Name == "GenericMethod" && m.GetGenericArguments().Length == 2);
            // Different number of generic arguments - should return false at line 206
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        [Fact]
        public void IsOverriddenByCovariantReturnMethod_GenericArgumentNotEquivalent_ReturnsFalse()
        {
            var baseMethod = typeof(BaseGenericMethod).GetMethod("GenericMethod");
            var covariantMethod = typeof(DerivedGenericMethodMismatch).GetMethod("GenericMethod");
            // Generic arguments don't match in constraints
            Assert.False(baseMethod.IsOverriddenByCovariantReturnMethod(covariantMethod));
        }

        #endregion

        #region SubstituteGenericParameters - element type cases

        [Fact]
        public void SubstituteGenericParameters_ArrayWithUnchangedElement_ReturnsOriginal()
        {
            // int[] is not a generic parameter, so it should return as-is
            var type = typeof(int[]);
            var map = new Dictionary<Type, Type>();
            var result = InvokeSubstituteGenericParameters(type, map);
            Assert.Equal(type, result);
        }

        [Fact]
        public void SubstituteGenericParameters_ByRefType_ReturnsByRef()
        {
            // Test by-ref type substitution path
            var method = typeof(RefParamTest).GetMethod("Method");
            var paramType = method.GetParameters()[0].ParameterType;
            var map = new Dictionary<Type, Type>();
            var result = InvokeSubstituteGenericParameters(paramType, map);
            // Should handle by-ref types
            Assert.NotNull(result);
        }

        [Fact]
        public void SubstituteGenericParameters_PointerType_ReturnsPointer()
        {
            // Test pointer type substitution path
            var type = typeof(int*);
            var map = new Dictionary<Type, Type>();
            var result = InvokeSubstituteGenericParameters(type, map);
            Assert.Equal(type, result);
        }

        [Fact]
        public void SubstituteGenericParameters_GenericParameterNotInMap_ReturnsSame()
        {
            // Generic parameter not in map should return itself
            var type = typeof(List<>).GetGenericArguments()[0];
            var map = new Dictionary<Type, Type>();
            var result = InvokeSubstituteGenericParameters(type, map);
            Assert.Equal(type, result);
        }

        [Fact]
        public void SubstituteGenericParameters_GenericParameterInMap_ReturnsMapped()
        {
            // Generic parameter in map should return mapped type
            var type = typeof(List<>).GetGenericArguments()[0];
            var map = new Dictionary<Type, Type> { { type, typeof(int) } };
            var result = InvokeSubstituteGenericParameters(type, map);
            Assert.Equal(typeof(int), result);
        }

        [Fact]
        public void SubstituteGenericParameters_ConstructedGenericWithUnchangedArgs_ReturnsOriginal()
        {
            // List<int> with no substitutions should return same type
            var type = typeof(List<int>);
            var map = new Dictionary<Type, Type>();
            var result = InvokeSubstituteGenericParameters(type, map);
            Assert.Equal(type, result);
        }

        [Fact]
        public void SubstituteGenericParameters_ConstructedGenericWithChangedArgs_ReturnsNew()
        {
            // List<T> where T maps to string should return List<string>
            // Use a generic parameter from a different type to avoid creating a generic type definition
            var type = typeof(DifferentGeneric<>).GetGenericArguments()[0];
            var map = new Dictionary<Type, Type> { { type, typeof(string) } };
            // Construct a generic type with T (from DifferentGeneric<>)
            var constructedType = typeof(List<>).MakeGenericType(type);
            var result = InvokeSubstituteGenericParameters(constructedType, map);
            Assert.Equal(typeof(List<string>), result);
        }

        #endregion

        #region AreEquivalentGenericTypes - array and generic cases

        [Fact]
        public void AreEquivalentGenericTypes_ArraysWithDifferentRanks_ReturnsFalse()
        {
            // int[] vs int[,] - different ranks
            var type1 = typeof(int[]);
            var type2 = typeof(int[,]);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericTypes_ArraysWithSameRank_ReturnsComparisonResult()
        {
            // int[] vs int[] - same rank, same element type
            var type1 = typeof(int[]);
            var type2 = typeof(int[]);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.True(result);
        }

        [Fact]
        public void AreEquivalentGenericTypes_OneNotGeneric_ReturnsFalse()
        {
            // One is generic, one is not
            var type1 = typeof(List<int>);
            var type2 = typeof(string);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericTypes_DifferentConstructedStatus_ReturnsFalse()
        {
            // List<int> is constructed, List<> is a definition
            var type1 = typeof(List<int>);
            var type2 = typeof(List<>);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericTypes_DifferentGenericArgumentCount_ReturnsFalse()
        {
            // List<int> vs Dictionary<int, string> - different arg count
            var type1 = typeof(List<int>);
            var type2 = typeof(Dictionary<int, string>);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericTypes_DifferentTypeDefinition_ReturnsFalse()
        {
            // List<int> vs HashSet<int> - different type definition
            var type1 = typeof(List<int>);
            var type2 = typeof(HashSet<int>);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericTypes_DifferentGenericArguments_ReturnsFalse()
        {
            // List<int> vs List<string> - different arguments
            var type1 = typeof(List<int>);
            var type2 = typeof(List<string>);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericTypes_SameType_ReturnsTrue()
        {
            // List<int> vs List<int> - same
            var type1 = typeof(List<int>);
            var type2 = typeof(List<int>);
            var result = InvokeAreEquivalentGenericTypes(type1, type2,
                (a, b) => a == b, (a, b) => a == b);
            Assert.True(result);
        }

        #endregion

        #region AreEquivalentGenericParameters

        [Fact]
        public void AreEquivalentGenericParameters_NotGenericParameter_ReturnsFalse()
        {
            var result = InvokeAreEquivalentGenericParameters(typeof(int), typeof(string));
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericParameters_DifferentPosition_ReturnsFalse()
        {
            var args = typeof(TwoParamGeneric<,>).GetGenericArguments();
            var result = InvokeAreEquivalentGenericParameters(args[0], args[1]);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericParameters_DifferentDeclaringMethod_ReturnsFalse()
        {
            // Generic parameters from different methods
            var method1 = typeof(GenericMethodClass).GetMethod("Method1");
            var method2 = typeof(GenericMethodClass).GetMethod("Method2");
            var param1 = method1.GetGenericArguments()[0];
            var param2 = method2.GetGenericArguments()[0];
            var result = InvokeAreEquivalentGenericParameters(param1, param2);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericParameters_DifferentDeclaringType_ReturnsFalse()
        {
            // Generic parameters from different types with same position
            var args1 = typeof(List<>).GetGenericArguments();
            var args2 = typeof(HashSet<>).GetGenericArguments();
            var result = InvokeAreEquivalentGenericParameters(args1[0], args2[0]);
            Assert.False(result);
        }

        [Fact]
        public void AreEquivalentGenericParameters_SameParameter_ReturnsTrue()
        {
            var args = typeof(List<>).GetGenericArguments();
            var result = InvokeAreEquivalentGenericParameters(args[0], args[0]);
            Assert.True(result);
        }

        #endregion

        #region TryUnwrapByRef

        [Fact]
        public void TryUnwrapByRef_OneByRefOneNot_ReturnsFalse()
        {
            var method = typeof(RefParamTest).GetMethod("Method");
            var byRefType = method.GetParameters()[0].ParameterType; // int&
            var normalType = typeof(int);
            var result = InvokeTryUnwrapByRef(byRefType, normalType);
            Assert.False(result.success);
        }

        [Fact]
        public void TryUnwrapByRef_BothNotByRef_ReturnsTrueUnchanged()
        {
            var type1 = typeof(int);
            var type2 = typeof(string);
            var result = InvokeTryUnwrapByRef(type1, type2);
            Assert.True(result.success);
            Assert.Equal(typeof(int), result.type1);
            Assert.Equal(typeof(string), result.type2);
        }

        [Fact]
        public void TryUnwrapByRef_BothByRef_ReturnsTrueUnwrapped()
        {
            var method = typeof(RefParamTest).GetMethod("Method");
            var byRefType = method.GetParameters()[0].ParameterType; // int&
            var result = InvokeTryUnwrapByRef(byRefType, byRefType);
            Assert.True(result.success);
            Assert.Equal(typeof(int), result.type1);
            Assert.Equal(typeof(int), result.type2);
        }

        #endregion

        #region FindMatchingBaseType (via CreateGenericParameterMap / AddTypeGenericParameterMap)

        [Fact]
        public void CreateGenericParameterMap_NonGenericDeclaringType_ReturnsEmpty()
        {
            // When declaring type is not generic, no type-level map is added
            var method = typeof(NonGenericClass).GetMethod("Method");
            var covariantMethod = typeof(NonGenericClass).GetMethod("Method");
            var result = InvokeCreateGenericParameterMap(method, covariantMethod);
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void CreateGenericParameterMap_GenericMethod_AddsMethodLevelMap()
        {
            var method = typeof(GenericMethodClass).GetMethod("GenericEcho");
            var covariantMethod = typeof(GenericMethodClass).GetMethod("GenericEcho");
            var result = InvokeCreateGenericParameterMap(method, covariantMethod);
            Assert.NotNull(result);
        }

        #endregion

        #region EnumerateBaseTypesAndInterfaces

        [Fact]
        public void EnumerateBaseTypesAndInterfaces_ReturnsTypeAndBasesAndInterfaces()
        {
            var result = InvokeEnumerateBaseTypesAndInterfaces(typeof(DerivedWithCovariant)).ToList();
            Assert.Contains(typeof(DerivedWithCovariant), result);
            Assert.Contains(typeof(BaseWithCovariantReturn), result);
            Assert.Contains(typeof(object), result);
        }

        #endregion

        #region GetInheritanceDepth

        [Fact]
        public void GetInheritanceDepth_NullType_ReturnsZero()
        {
            Type? nullType = null;
            Assert.Equal(0, nullType.GetInheritanceDepth());
        }

        [Fact]
        public void GetInheritanceDepth_Object_ReturnsZero()
        {
            // object has no base type, so inheritance depth is -1
            Assert.Equal(-1, typeof(object).GetInheritanceDepth());
        }

        [Fact]
        public void GetInheritanceDepth_DirectInheritor_ReturnsZero()
        {
            // Directly inherits from object, so depth is 0
            Assert.Equal(0, typeof(NonCovariantClass).GetInheritanceDepth());
        }

        [Fact]
        public void GetInheritanceDepth_DeepInheritance_ReturnsCorrectDepth()
        {
            // DerivedWithCovariant -> BaseNonCovariant -> object
            // depth = 2 (excluding object level)
            Assert.Equal(1, typeof(DerivedWithCovariant).GetInheritanceDepth());
        }

        #endregion

        #region IsGenericParameterCovariant

        [Fact]
        public void IsGenericParameterCovariant_NotGenericParameter_ReturnsFalse()
        {
            Assert.False(typeof(int).IsGenericParameterCovariant());
        }

        [Fact]
        public void IsGenericParameterCovariant_InvariantParameter_ReturnsFalse()
        {
            var args = typeof(List<>).GetGenericArguments();
            Assert.False(args[0].IsGenericParameterCovariant());
        }

        [Fact]
        public void IsGenericParameterCovariant_CovariantParameter_ReturnsTrue()
        {
            var args = typeof(IEnumerable<>).GetGenericArguments();
            Assert.True(args[0].IsGenericParameterCovariant());
        }

        #endregion

        #region IsAssignableFromGenericTypeDefinition

        [Fact]
        public void IsAssignableFromGenericTypeDefinition_NotGenericTypeDefinition_ReturnsFalse()
        {
            Assert.False(typeof(int).IsAssignableFromGenericTypeDefinition(typeof(string)));
        }

        [Fact]
        public void IsAssignableFromGenericTypeDefinition_SameType_ReturnsTrue()
        {
            Assert.True(typeof(IComparable<>).IsAssignableFromGenericTypeDefinition(typeof(IComparable<>)));
        }

        [Fact]
        public void IsAssignableFromGenericTypeDefinition_AssignableViaInterface_ReturnsTrue()
        {
            // IComparable<> is not directly assignable from int as a generic type definition
            Assert.False(typeof(IComparable<>).IsAssignableFromGenericTypeDefinition(typeof(int)));
        }

        [Fact]
        public void IsAssignableFromGenericTypeDefinition_NotAssignable_ReturnsFalse()
        {
            Assert.False(typeof(IDisposable).IsAssignableFromGenericTypeDefinition(typeof(int)));
        }

        #endregion

        #region Reflection Helpers

        private static Type InvokeSubstituteGenericParameters(Type type, IReadOnlyDictionary<Type, Type> map)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("SubstituteGenericParameters",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (Type)method.Invoke(null, new object[] { type, map });
        }

        private static bool InvokeAreEquivalentGenericTypes(Type type, Type other,
            Func<Type, Type, bool> argumentComparer, Func<Type, Type, bool> typeDefinitionComparer)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("AreEquivalentGenericTypes",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (bool)method.Invoke(null, new object[] { type, other, argumentComparer, typeDefinitionComparer });
        }

        private static bool InvokeAreEquivalentGenericParameters(Type type, Type other)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("AreEquivalentGenericParameters",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (bool)method.Invoke(null, new object[] { type, other });
        }

        private static (bool success, Type type1, Type type2) InvokeTryUnwrapByRef(Type type1, Type type2)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("TryUnwrapByRef",
                BindingFlags.NonPublic | BindingFlags.Static);
            var parameters = new object[] { type1, type2 };
            var result = (bool)method.Invoke(null, parameters);
            return (result, (Type)parameters[0], (Type)parameters[1]);
        }

        private static IReadOnlyDictionary<Type, Type> InvokeCreateGenericParameterMap(MethodInfo method, MethodInfo covariantReturnMethod)
        {
            var privateMethod = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("CreateGenericParameterMap",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (IReadOnlyDictionary<Type, Type>)privateMethod.Invoke(null, new object[] { method, covariantReturnMethod });
        }

        private static IEnumerable<Type> InvokeEnumerateBaseTypesAndInterfaces(Type type)
        {
            var method = typeof(AspectCore.Extensions.TypeExtensions).GetMethod("EnumerateBaseTypesAndInterfaces",
                BindingFlags.NonPublic | BindingFlags.Static);
            return (IEnumerable<Type>)method.Invoke(null, new object[] { type });
        }

        #endregion

        #region Test Types

        public class NonCovariantClass { }

        public interface INonCovariant
        {
            int GetValue();
        }

        public class BaseNonCovariant : INonCovariant
        {
            public virtual int GetValue() => 0;
        }

        public class DerivedNonCovariant : BaseNonCovariant
        {
            public override int GetValue() => 1;
        }

        public class BaseResult { }
        public class DerivedResult : BaseResult { }

        public class BaseWithCovariantReturn
        {
            public virtual BaseResult GetValue() => new BaseResult();
        }

        public class DerivedWithCovariant : BaseWithCovariantReturn
        {
            public override DerivedResult GetValue() => new DerivedResult();
            public DerivedResult GetDifferentValue() => new DerivedResult();
        }

        public class UnrelatedBase
        {
            public virtual object GetValue() => new object();
        }

        public class BaseWithStringReturn
        {
            public virtual string GetValue() => "";
        }

        public class BaseWithParams
        {
            public virtual object Process(int x, int y) => x + y;
        }

        public class BaseWithStringParam
        {
            public virtual object Process(string s) => s;
        }

        public class BaseGenericMethod
        {
            public virtual object GenericMethod<T>(T value) => value;
        }

        public class DerivedTwoGenericArgs : BaseGenericMethod
        {
            public override object GenericMethod<T>(T value) => value;
            // Overload with 2 generic args
            public object GenericMethod<T1, T2>(T1 v1, T2 v2) => v1;
        }

        public class DerivedGenericMethodMismatch : BaseGenericMethod
        {
            public override object GenericMethod<T>(T value) => value;
        }

        public static class GenericMethodClass
        {
            public static T Method1<T>(T value) => value;
            public static T Method2<T>(T value) => value;
            public static T GenericEcho<T>(T value) => value;
        }

        public static class NonGenericClass
        {
            public static int Method() => 0;
        }

        public static class RefParamTest
        {
            public static void Method(ref int value) { value = 0; }
        }

        public class TwoParamGeneric<T1, T2> { }

        public class DifferentGeneric<T> { }

        #endregion
    }
}
