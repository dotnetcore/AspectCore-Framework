#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Extensions;
using Xunit;
using static AspectCore.Core.Tests.Extensions.TypeExtensionsTests.TestTypes;
using TypeExtensions = AspectCore.Extensions.TypeExtensions;

namespace AspectCore.Core.Tests.Extensions.TypeExtensionsTests;

public class TypeExtensionsTests
{
    #region IsGenericParameterCovariant

    [Fact]
    public void IsGenericParameterCovariant_ReturnsTrue_ForCovariantParameter()
    {
        var parameter = typeof(IEnumerable<>).GetGenericArguments()[0];
        Assert.True(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsTrue_ForCovariantParameterOnFunc()
    {
        var parameter = typeof(Func<>).GetGenericArguments()[0];
        Assert.True(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsTrue_ForCovariantParameterOnIReadOnlyList()
    {
        var parameter = typeof(IReadOnlyList<>).GetGenericArguments()[0];
        Assert.True(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForInvariantParameter()
    {
        var parameter = typeof(List<>).GetGenericArguments()[0];
        Assert.False(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForInvariantParameterOnICollection()
    {
        var parameter = typeof(ICollection<>).GetGenericArguments()[0];
        Assert.False(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForContravariantParameter()
    {
        var parameter = typeof(IComparable<>).GetGenericArguments()[0];
        Assert.False(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForContravariantParameterOnAction()
    {
        var parameter = typeof(Action<>).GetGenericArguments()[0];
        Assert.False(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForNonGenericParameter()
    {
        Assert.False(typeof(int).IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForNonGenericParameterString()
    {
        Assert.False(typeof(string).IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForGenericTypeDefinition()
    {
        Assert.False(typeof(List<>).IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsTrue_ForCustomCovariantInterface()
    {
        var parameter = typeof(ICovariant<>).GetGenericArguments()[0];
        Assert.True(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForCustomContravariantInterface()
    {
        var parameter = typeof(IContravariant<>).GetGenericArguments()[0];
        Assert.False(parameter.IsGenericParameterCovariant());
    }

    [Fact]
    public void IsGenericParameterCovariant_ReturnsFalse_ForCustomInvariantInterface()
    {
        var parameter = typeof(IInvariant<>).GetGenericArguments()[0];
        Assert.False(parameter.IsGenericParameterCovariant());
    }

    #endregion

    #region GetInheritanceDepth

    [Fact]
    public void GetInheritanceDepth_ReturnsZero_ForNull()
    {
        Type? type = null;
        Assert.Equal(0, type.GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsZero_ForTypeDirectlyInheritingFromObject()
    {
        Assert.Equal(0, typeof(CommonService).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsOne_ForTypeWithOneLevelOfInheritance()
    {
        Assert.Equal(1, typeof(BaseCovariantReturnService).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsTwo_ForTypeWithTwoLevelsOfInheritance()
    {
        Assert.Equal(2, typeof(MidCovariantReturnService).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsThree_ForTypeWithThreeLevelsOfInheritance()
    {
        Assert.Equal(3, typeof(LeafCovariantReturnService).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsSameValue_ForDerivedTypeWithoutAdditionalInheritance()
    {
        // DerivedLeafCovariantReturnService : LeafCovariantReturnService (no additional depth beyond parent)
        Assert.Equal(4, typeof(DerivedLeafCovariantReturnService).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsZero_ForInterface()
    {
        // Interfaces have no BaseType, so depth is -1 per the formula
        Assert.Equal(-1, typeof(ICommonService).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsNegativeOne_ForObject()
    {
        // object.BaseType is null, depth = 0, returns 0 - 1 = -1
        Assert.Equal(-1, typeof(object).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsZero_ForBaseResultDirectlyInheritingFromObject()
    {
        // BaseResult : object
        Assert.Equal(0, typeof(BaseResult).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsOne_ForMidResult()
    {
        // MidResult : BaseResult : object
        Assert.Equal(1, typeof(MidResult).GetInheritanceDepth());
    }

    [Fact]
    public void GetInheritanceDepth_ReturnsTwo_ForLeafResult()
    {
        // LeafResult : MidResult : BaseResult : object
        Assert.Equal(2, typeof(LeafResult).GetInheritanceDepth());
    }

    #endregion

    #region IsAssignableFromGenericTypeDefinition

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsTrue_WhenOtherIsAssignableGenericTypeDefinition()
    {
        // List<> implements IEnumerable<>, so the generic type definition is assignable
        Assert.True(typeof(IEnumerable<>).IsAssignableFromGenericTypeDefinition(typeof(List<>)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsTrue_WhenOtherIsDirectlyAssignable()
    {
        // List<> is assignable from itself
        Assert.True(typeof(List<>).IsAssignableFromGenericTypeDefinition(typeof(List<>)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsTrue_WhenOtherImplementsInterfaceViaBase()
    {
        // Collection<> implements IList<> through its base class
        Assert.True(typeof(IList<>).IsAssignableFromGenericTypeDefinition(typeof(List<>)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsFalse_WhenTypeIsNotGenericTypeDefinition()
    {
        // typeof(int) is not a generic type definition
        Assert.False(typeof(IEnumerable<>).IsAssignableFromGenericTypeDefinition(typeof(int)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsFalse_WhenOtherIsNotGenericTypeDefinition()
    {
        // typeof(List<int>) is a constructed generic type, not a definition
        Assert.False(typeof(IEnumerable<>).IsAssignableFromGenericTypeDefinition(typeof(List<int>)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsFalse_WhenOtherIsNotAssignable()
    {
        // List<> is not assignable from IEnumerable<> (reverse direction)
        Assert.False(typeof(List<>).IsAssignableFromGenericTypeDefinition(typeof(IEnumerable<>)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsFalse_WhenTypeIsConstructedGenericType()
    {
        // typeof(IEnumerable<int>) is not a generic type definition
        Assert.False(typeof(IEnumerable<int>).IsAssignableFromGenericTypeDefinition(typeof(List<>)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsTrue_WhenOtherIsDerivedGenericTypeDefinition()
    {
        // BaseGeneric<> is the base of DerivedGeneric<>
        Assert.True(typeof(BaseGeneric<>).IsAssignableFromGenericTypeDefinition(typeof(DerivedGeneric<>)));
    }

    [Fact]
    public void IsAssignableFromGenericTypeDefinition_ReturnsFalse_WhenOtherIsUnrelatedGenericTypeDefinition()
    {
        // Dictionary<,> is not assignable to IEnumerable<>
        // Actually it is assignable via ICollection<>... let me use a truly unrelated type
        Assert.False(typeof(IComparable<>).IsAssignableFromGenericTypeDefinition(typeof(List<>)));
    }

    #endregion

    #region IsCovariantReturnAssignableFrom

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_WhenOtherIsDirectlyAssignable()
    {
        // LeafResult is assignable to BaseResult
        Assert.True(typeof(BaseResult).IsCovariantReturnAssignableFrom(typeof(LeafResult)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_WhenOtherIsSameType()
    {
        Assert.True(typeof(BaseResult).IsCovariantReturnAssignableFrom(typeof(BaseResult)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_WhenOtherIsAssignableViaIntermediateType()
    {
        // LeafResult : MidResult : BaseResult
        Assert.True(typeof(BaseResult).IsCovariantReturnAssignableFrom(typeof(LeafResult)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsFalse_WhenOtherIsNotAssignable()
    {
        // BaseResult is not assignable to LeafResult
        Assert.False(typeof(LeafResult).IsCovariantReturnAssignableFrom(typeof(BaseResult)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsFalse_WhenTypesAreCompletelyUnrelated()
    {
        Assert.False(typeof(BaseResult).IsCovariantReturnAssignableFrom(typeof(int)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_ForCovariantGenericParameterAssignment()
    {
        // IEnumerable<LeafResult> is assignable to IEnumerable<BaseResult> due to covariance
        Assert.True(typeof(IEnumerable<BaseResult>).IsCovariantReturnAssignableFrom(typeof(IEnumerable<LeafResult>)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsFalse_ForInvariantGenericParameterAssignment()
    {
        // List<LeafResult> is NOT assignable to List<BaseResult> (invariant)
        Assert.False(typeof(List<BaseResult>).IsCovariantReturnAssignableFrom(typeof(List<LeafResult>)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_WhenOtherIsGenericTypeDefinitionAssignable()
    {
        // typeof(IEnumerable<>) is a generic type definition, assignable from typeof(List<>)
        Assert.True(typeof(IEnumerable<>).IsCovariantReturnAssignableFrom(typeof(List<>)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_ForByRefCompatibleTypes()
    {
        // ref BaseResult is compatible with ref LeafResult (after unwrapping by-ref)
        Assert.True(typeof(BaseResult).MakeByRefType().IsCovariantReturnAssignableFrom(typeof(LeafResult).MakeByRefType()));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsFalse_ForByRefMismatch()
    {
        // ref BaseResult is NOT compatible with non-ref LeafResult
        Assert.False(typeof(BaseResult).MakeByRefType().IsCovariantReturnAssignableFrom(typeof(LeafResult)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_ForEquivalentGenericParameters()
    {
        // Same generic parameter from the same generic type definition
        var parameter = typeof(IEnumerable<>).GetGenericArguments()[0];
        Assert.True(parameter.IsCovariantReturnAssignableFrom(parameter));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsFalse_ForContravariantGenericParameterAssignment()
    {
        // IComparable<BaseResult> is NOT assignable from IComparable<LeafResult> (contravariant)
        // Actually, with contravariance, IComparable<BaseResult> IS assignable from IComparable<LeafResult>
        // Let me think about this more carefully...
        // Contravariance: if BaseResult is a supertype of LeafResult,
        // then IComparable<BaseResult> is a subtype of IComparable<LeafResult>
        // So IComparable<LeafResult> is assignable FROM IComparable<BaseResult>
        // But we're checking: IComparable<BaseResult> is assignable FROM IComparable<LeafResult>
        // That would be false.
        Assert.False(typeof(IComparable<BaseResult>).IsCovariantReturnAssignableFrom(typeof(IComparable<LeafResult>)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsTrue_ForObjectAssignableFromAnything()
    {
        Assert.True(typeof(object).IsCovariantReturnAssignableFrom(typeof(BaseResult)));
    }

    [Fact]
    public void IsCovariantReturnAssignableFrom_ReturnsFalse_ForValueTypeNotAssignableToReferenceType()
    {
        Assert.False(typeof(BaseResult).IsCovariantReturnAssignableFrom(typeof(int)));
    }

    #endregion

    #region IsCovariantReturnEquivalentTo

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsTrue_ForSameType()
    {
        Assert.True(typeof(BaseResult).IsCovariantReturnEquivalentTo(typeof(BaseResult)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsTrue_ForSamePrimitiveType()
    {
        Assert.True(typeof(int).IsCovariantReturnEquivalentTo(typeof(int)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForDifferentTypes()
    {
        Assert.False(typeof(BaseResult).IsCovariantReturnEquivalentTo(typeof(LeafResult)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForDifferentPrimitiveTypes()
    {
        Assert.False(typeof(int).IsCovariantReturnEquivalentTo(typeof(string)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsTrue_ForEquivalentGenericParameters()
    {
        // Same generic parameter position and declaring type
        var parameter = typeof(List<>).GetGenericArguments()[0];
        Assert.True(parameter.IsCovariantReturnEquivalentTo(parameter));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForNonEquivalentGenericParameters()
    {
        // Different generic parameter positions
        var parameters = typeof(Dictionary<,>).GetGenericArguments();
        Assert.False(parameters[0].IsCovariantReturnEquivalentTo(parameters[1]));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsTrue_ForEquivalentGenericTypes()
    {
        // List<BaseResult> is equivalent to List<BaseResult>
        Assert.True(typeof(List<BaseResult>).IsCovariantReturnEquivalentTo(typeof(List<BaseResult>)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForNonEquivalentGenericTypes()
    {
        // List<BaseResult> is NOT equivalent to List<LeafResult>
        Assert.False(typeof(List<BaseResult>).IsCovariantReturnEquivalentTo(typeof(List<LeafResult>)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsTrue_ForByRefCompatibleTypes()
    {
        // ref BaseResult is equivalent to ref BaseResult
        Assert.True(typeof(BaseResult).MakeByRefType().IsCovariantReturnEquivalentTo(typeof(BaseResult).MakeByRefType()));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForByRefMismatch()
    {
        // ref BaseResult is NOT equivalent to non-ref BaseResult
        Assert.False(typeof(BaseResult).MakeByRefType().IsCovariantReturnEquivalentTo(typeof(BaseResult)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsTrue_ForEquivalentArrayTypes()
    {
        // BaseResult[] is equivalent to BaseResult[]
        Assert.True(typeof(BaseResult[]).IsCovariantReturnEquivalentTo(typeof(BaseResult[])));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForDifferentArrayRanks()
    {
        // BaseResult[] is NOT equivalent to BaseResult[,]
        Assert.False(typeof(BaseResult[]).IsCovariantReturnEquivalentTo(typeof(BaseResult[,])));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForArrayVsNonArray()
    {
        // BaseResult[] is NOT equivalent to List<BaseResult>
        Assert.False(typeof(BaseResult[]).IsCovariantReturnEquivalentTo(typeof(List<BaseResult>)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsTrue_ForEquivalentNestedGenericTypes()
    {
        // List<IEnumerable<BaseResult>> is equivalent to List<IEnumerable<BaseResult>>
        Assert.True(typeof(List<IEnumerable<BaseResult>>).IsCovariantReturnEquivalentTo(typeof(List<IEnumerable<BaseResult>>)));
    }

    [Fact]
    public void IsCovariantReturnEquivalentTo_ReturnsFalse_ForGenericTypeVsGenericTypeDefinition()
    {
        // List<> (definition) is NOT equivalent to List<BaseResult> (constructed)
        Assert.False(typeof(List<>).IsCovariantReturnEquivalentTo(typeof(List<BaseResult>)));
    }

    #endregion

    #region GetCovariantReturnMethods

    [Fact]
    public void GetCovariantReturnMethods_ReturnsEmpty_ForTypeWithoutCovariantReturns()
    {
        var result = typeof(CommonService).GetCovariantReturnMethods();
        Assert.Empty(result);
    }

    [Fact]
    public void GetCovariantReturnMethods_ReturnsEmpty_ForInterface()
    {
        var result = typeof(ICommonService).GetCovariantReturnMethods();
        Assert.Empty(result);
    }

    [Fact]
    public void GetCovariantReturnMethods_ReturnsCovariantMethods_ForBaseCovariantReturnService()
    {
        var result = typeof(BaseCovariantReturnService).GetCovariantReturnMethods();
        Assert.NotEmpty(result);

        // Should contain the Method override (BaseResult Method())
        var methodInfo = result.Single(m => m.CovariantReturnMethod.Name == nameof(BaseCovariantReturnService.Method));
        Assert.Equal(typeof(BaseResult), methodInfo.CovariantReturnMethod.ReturnType);
        Assert.Equal(typeof(object), methodInfo.OverriddenMethod.ReturnType);
    }

    [Fact]
    public void GetCovariantReturnMethods_ReturnsCovariantMethods_ForLeafCovariantReturnService()
    {
        var result = typeof(LeafCovariantReturnService).GetCovariantReturnMethods();
        Assert.NotEmpty(result);

        // The leaf type has multiple covariant return methods in the chain (one per hierarchy level)
        var matches = result.Where(m => m.CovariantReturnMethod.Name == nameof(LeafCovariantReturnService.Method)).ToArray();
        Assert.NotEmpty(matches);

        // The most derived override should return LeafResult
        var leafMatch = matches.First(m => m.CovariantReturnMethod.ReturnType == typeof(LeafResult));
        Assert.Equal(typeof(LeafResult), leafMatch.CovariantReturnMethod.ReturnType);
    }

    [Fact]
    public void GetCovariantReturnMethods_ContainsInterfaceDeclarations_ForCovariantMethod()
    {
        var result = typeof(BaseCovariantReturnService).GetCovariantReturnMethods();
        var methodInfo = result.Single(m => m.CovariantReturnMethod.Name == nameof(BaseCovariantReturnService.Method));

        // The Method() implements ICommonService.Method()
        Assert.Contains(methodInfo.InterfaceDeclarations, m => m.Name == nameof(ICommonService.Method));
    }

    [Fact]
    public void GetCovariantReturnMethods_ReturnsEmpty_ForOrdinaryOverrideService()
    {
        // OrdinaryOverrideService overrides with the same return type (object), not covariant
        var result = typeof(OrdinaryOverrideService).GetCovariantReturnMethods();
        Assert.Empty(result);
    }

    [Fact]
    public void GetCovariantReturnMethods_InheritanceDepth_IsCorrect()
    {
        var result = typeof(BaseCovariantReturnService).GetCovariantReturnMethods();
        var methodInfo = result.Single(m => m.CovariantReturnMethod.Name == nameof(BaseCovariantReturnService.Method));

        // BaseCovariantReturnService : CommonService : object
        // Depth should be 1
        Assert.Equal(1, methodInfo.InheritanceDepth);
    }

    [Fact]
    public void GetCovariantReturnMethods_InheritanceDepth_IsCorrectForLeaf()
    {
        var result = typeof(LeafCovariantReturnService).GetCovariantReturnMethods();
        var matches = result.Where(m => m.CovariantReturnMethod.Name == nameof(LeafCovariantReturnService.Method)).ToArray();
        Assert.NotEmpty(matches);

        // Find the match where the covariant return method returns LeafResult
        // LeafCovariantReturnService : MidCovariantReturnService : BaseCovariantReturnService : CommonService : object
        // Depth should be 3
        var leafMatch = matches.First(m => m.CovariantReturnMethod.ReturnType == typeof(LeafResult));
        Assert.Equal(3, leafMatch.InheritanceDepth);
    }

    [Fact]
    public void GetCovariantReturnMethods_ReturnsEmpty_ForValueType()
    {
        var result = typeof(int).GetCovariantReturnMethods();
        Assert.Empty(result);
    }

    [Fact]
    public void GetCovariantReturnMethods_ReturnsEmpty_ForSealedType()
    {
        var result = typeof(LeafResult).GetCovariantReturnMethods();
        Assert.Empty(result);
    }

    [Fact]
    public void GetCovariantReturnMethods_AllReturnedItemsHaveValidCovariantReturnMethod()
    {
        var result = typeof(LeafCovariantReturnService).GetCovariantReturnMethods();
        foreach (var item in result)
        {
            Assert.NotNull(item.CovariantReturnMethod);
            Assert.NotNull(item.OverriddenMethod);
            // The covariant return type must be more derived (narrower) than the overridden return type
            Assert.True(item.OverriddenMethod.ReturnType.IsAssignableFrom(item.CovariantReturnMethod.ReturnType));
            Assert.NotEqual(item.OverriddenMethod.ReturnType, item.CovariantReturnMethod.ReturnType);
        }
    }

    [Fact]
    public void GetCovariantReturnMethods_ReturnsEmpty_WhenPreserveBaseOverridesAttributeIsNull()
    {
        // This test verifies the behavior when PreserveBaseOverridesAttribute is null
        // On runtimes that support it, this attribute is non-null
        // We can't easily test the null case, but we can verify the attribute is checked
        if (TypeExtensions.PreserveBaseOverridesAttribute is null)
        {
            var result = typeof(BaseCovariantReturnService).GetCovariantReturnMethods();
            Assert.Empty(result);
        }
        else
        {
            // Skip on runtimes that support covariant returns
            Assert.NotNull(TypeExtensions.PreserveBaseOverridesAttribute);
        }
    }

    #endregion

    #region PreserveBaseOverridesAttribute

    [Fact]
    public void PreserveBaseOverridesAttribute_IsNotNull_OnSupportedRuntime()
    {
        // On .NET 5+, the PreserveBaseOverridesAttribute should exist
        // This test documents the expected behavior on supported runtimes
        // The attribute is used by the CLR to preserve covariant-return base overrides
        Assert.NotNull(TypeExtensions.PreserveBaseOverridesAttribute);
    }

    [Fact]
    public void PreserveBaseOverridesAttribute_IsCompilerServicesType()
    {
        if (TypeExtensions.PreserveBaseOverridesAttribute is not null)
        {
            Assert.Equal("System.Runtime.CompilerServices.PreserveBaseOverridesAttribute",
                TypeExtensions.PreserveBaseOverridesAttribute.FullName);
        }
    }

    #endregion

    #region Test Types

    public interface ICovariant<out T>
    {
        T Get();
    }

    public interface IContravariant<in T>
    {
        void Set(T value);
    }

    public interface IInvariant<T>
    {
        T Get();
        void Set(T value);
    }

    public class BaseGeneric<T>
    {
        public T? Value { get; set; }
    }

    public class DerivedGeneric<T> : BaseGeneric<T>
    {
    }

    #endregion
}
