using System.Linq;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using AspectCore.Extensions;
using Xunit;
using Xunit.Abstractions;
using static AspectCore.Tests.CovariantReturnTypes;

namespace AspectCore.Tests.Extensions.TypeExtensionsTests;

public class IsOverriddenByCovariantReturnMethodTests(ITestOutputHelper output)
{
    [Fact]
    public void ShouldReturnTrue_WhenMethodIsOverriddenWithCovariantReturnType()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantReturnMethod = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenMethodNamesDiffer()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantReturnMethod = GetPropertyGetter<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Property), typeof(BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenReturnTypesAreTheSame()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));
        var covariantReturnMethod = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenReturnTypeIsNotAssignable()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));
        var covariantReturnMethod = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenParametersDiffer()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantReturnMethod = GetMethod<GenericMethodLeafService>(nameof(GenericMethodLeafService.Convert), typeof(LeafResult), parameterCount: 1);

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenGenericMethodIsOverriddenWithCovariantReturnType()
    {
        var method = GetMethod<GenericMethodBaseService>(nameof(GenericMethodBaseService.Convert), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<GenericMethodLeafService>(nameof(GenericMethodLeafService.Convert), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenBothArgumentsAreSameMethodInfoInstance()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(method));
    }

    [Fact]
    public void ShouldReturnFalse_WhenBothArgumentsAreSameMethodReflectedFromDifferentTypes()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var reflectedMethod = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(object));

        Assert.Equal(method.MetadataToken, reflectedMethod.MetadataToken);
        Assert.Equal(method.Module, reflectedMethod.Module);
        Assert.NotEqual(method.ReflectedType, reflectedMethod.ReflectedType);
        Assert.False(method.IsOverriddenByCovariantReturnMethod(reflectedMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenSecondMethodIsOrdinaryOverride()
    {
        var method = GetMethod<OrdinaryOverrideBaseService>(nameof(OrdinaryOverrideBaseService.Method), typeof(BaseResult));
        var ordinaryOverrideMethod = GetMethod<OrdinaryOverrideLeafService>(nameof(OrdinaryOverrideLeafService.Method), typeof(BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(ordinaryOverrideMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenSecondMethodIsOrdinaryOverrideOfCovariantLeaf()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));
        var ordinaryOverrideMethod = GetMethod<DerivedLeafCovariantReturnService>(nameof(DerivedLeafCovariantReturnService.Method), typeof(LeafResult));

        Assert.True(ordinaryOverrideMethod.GetBaseDefinition() == GetMethod<LeafCovariantReturnService>(nameof(LeafCovariantReturnService.Method), typeof(LeafResult)).GetBaseDefinition());
        Assert.False(method.IsOverriddenByCovariantReturnMethod(ordinaryOverrideMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenCovariantPropertyGetterOverridesBaseGetter()
    {
        var method = GetPropertyGetter<CommonService>(nameof(CommonService.Property), typeof(object));
        var covariantReturnMethod = GetPropertyGetter<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Property), typeof(BaseResult));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenLeafCovariantMethodOverridesObjectReturnMethod()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantReturnMethod = GetMethod<LeafCovariantReturnService>(nameof(LeafCovariantReturnService.Method), typeof(LeafResult));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenLeafCovariantMethodOverridesIntermediateBaseResultMethod()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));
        var covariantReturnMethod = GetMethod<LeafCovariantReturnService>(nameof(LeafCovariantReturnService.Method), typeof(LeafResult));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenNonGenericParametersMatch()
    {
        var method = GetMethod<ParameterBaseService>(nameof(ParameterBaseService.WithBaseParameter), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<ParameterLeafService>(nameof(ParameterLeafService.WithBaseParameter), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenMultipleNonGenericParametersMatch()
    {
        var method = GetMethod<ParameterBaseService>(nameof(ParameterBaseService.WithTwoParameters), typeof(BaseResult), parameterCount: 2);
        var covariantReturnMethod = GetMethod<ParameterLeafService>(nameof(ParameterLeafService.WithTwoParameters), typeof(LeafResult), parameterCount: 2);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenParameterTypesDiffer()
    {
        var method = GetMethod<ParameterBaseService>(nameof(ParameterBaseService.WithBaseParameter), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<MismatchedParameterLeafService>(nameof(MismatchedParameterLeafService.WithBaseParameter), typeof(LeafResult), parameterCount: 1);

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenGenericMethodParameterIsArrayOfMethodGenericParameter()
    {
        var method = GetMethod<GenericMethodShapeBaseService>(nameof(GenericMethodShapeBaseService.Array), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<GenericMethodShapeLeafService>(nameof(GenericMethodShapeLeafService.Array), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenGenericMethodParameterIsListOfMethodGenericParameter()
    {
        var method = GetMethod<GenericMethodShapeBaseService>(nameof(GenericMethodShapeBaseService.List), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<GenericMethodShapeLeafService>(nameof(GenericMethodShapeLeafService.List), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenGenericMethodParameterIsDictionaryContainingMethodGenericParameter()
    {
        var method = GetMethod<GenericMethodShapeBaseService>(nameof(GenericMethodShapeBaseService.Dictionary), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<GenericMethodShapeLeafService>(nameof(GenericMethodShapeLeafService.Dictionary), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenGenericMethodParameterIsByRefMethodGenericParameter()
    {
        var method = GetMethod<GenericMethodShapeBaseService>(nameof(GenericMethodShapeBaseService.ByRef), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<GenericMethodShapeLeafService>(nameof(GenericMethodShapeLeafService.ByRef), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenGenericMethodReturnTypeContainsMethodGenericParameter()
    {
        var method = GetGenericReturnMethod<GenericMethodShapeBaseService>(nameof(GenericMethodShapeBaseService.ReturnList), typeof(System.Collections.Generic.IEnumerable<>));
        var covariantReturnMethod = GetGenericReturnMethod<GenericMethodShapeLeafService>(nameof(GenericMethodShapeLeafService.ReturnList), typeof(System.Collections.Generic.List<>));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenParameterIsTypeGenericParameter()
    {
        var method = GetMethod(typeof(TypeGenericShapeBaseService<>), nameof(TypeGenericShapeBaseService<object>.Direct), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod(typeof(TypeGenericShapeLeafService<>), nameof(TypeGenericShapeLeafService<object>.Direct), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenParameterIsListOfTypeGenericParameter()
    {
        var method = GetMethod(typeof(TypeGenericShapeBaseService<>), nameof(TypeGenericShapeBaseService<object>.List), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod(typeof(TypeGenericShapeLeafService<>), nameof(TypeGenericShapeLeafService<object>.List), typeof(LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenReturnTypeContainsTypeGenericParameter()
    {
        var method = GetGenericReturnMethod(typeof(TypeGenericShapeBaseService<>), nameof(TypeGenericShapeBaseService<object>.ReturnList), typeof(System.Collections.Generic.IEnumerable<>));
        var covariantReturnMethod = GetGenericReturnMethod(typeof(TypeGenericShapeLeafService<>), nameof(TypeGenericShapeLeafService<object>.ReturnList), typeof(System.Collections.Generic.List<>));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenParametersContainTypeAndMethodGenericParameters()
    {
        var method = GetMethod(typeof(MixedGenericShapeBaseService<>), nameof(MixedGenericShapeBaseService<object>.TypeAndMethod), typeof(BaseResult), parameterCount: 2);
        var covariantReturnMethod = GetMethod(typeof(MixedGenericShapeLeafService<>), nameof(MixedGenericShapeLeafService<object>.TypeAndMethod), typeof(LeafResult), parameterCount: 2);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenMethodGenericParameterPrecedesTypeGenericParameter()
    {
        var method = GetMethod(typeof(MixedGenericShapeBaseService<>), nameof(MixedGenericShapeBaseService<object>.MethodThenType), typeof(BaseResult), parameterCount: 2);
        var covariantReturnMethod = GetMethod(typeof(MixedGenericShapeLeafService<>), nameof(MixedGenericShapeLeafService<object>.MethodThenType), typeof(LeafResult), parameterCount: 2);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenTypeGenericParameterIsComparedWithMethodGenericParameter()
    {
        var method = GetMethod(typeof(TypeGenericParameterBaseService<>), nameof(TypeGenericParameterBaseService<object>.Compare), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<MethodGenericParameterLeafService>(nameof(MethodGenericParameterLeafService.Compare), typeof(LeafResult), parameterCount: 1);

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnFalse_WhenGenericParameterPositionsDiffer()
    {
        var method = GetMethod<GenericPositionZeroBaseService>(nameof(GenericPositionZeroBaseService.Compare), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<GenericPositionOneLeafService>(nameof(GenericPositionOneLeafService.Compare), typeof(LeafResult), parameterCount: 1);

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void ShouldReturnTrue_WhenCovariantReturnUsesConstrainedGenericParameter()
    {
        var method = GetMethod<ConstrainedGenericReturnBaseService>(nameof(ConstrainedGenericReturnBaseService.Create), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<ConstrainedGenericReturnLeafService>(
            nameof(ConstrainedGenericReturnLeafService.Create),
            method => method.ReturnType.IsGenericParameter && method.GetParameters().Length == 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    private static MethodInfo GetMethod<T>(string name, Type returnType, int parameterCount = 0)
    {
        return GetMethod(typeof(T), name, returnType, parameterCount);
    }

    private static MethodInfo GetMethod(Type type, string name, Type returnType, int parameterCount = 0)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method =>
                method.Name == name
                && method.ReturnType == returnType
                && method.GetParameters().Length == parameterCount);
    }

    private static MethodInfo GetMethod<T>(string name, Func<MethodInfo, bool> predicate)
    {
        return GetMethod(typeof(T), name, predicate);
    }

    private static MethodInfo GetMethod(Type type, string name, Func<MethodInfo, bool> predicate)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method => method.Name == name && predicate(method));
    }

    private static MethodInfo GetGenericReturnMethod<T>(string name, Type genericTypeDefinition)
    {
        return GetGenericReturnMethod(typeof(T), name, genericTypeDefinition);
    }

    private static MethodInfo GetGenericReturnMethod(Type type, string name, Type genericTypeDefinition)
    {
        return GetMethod(type, name, method =>
            method.ReturnType.IsGenericType
            && method.ReturnType.GetGenericTypeDefinition() == genericTypeDefinition);
    }

    private static MethodInfo GetPropertyGetter<T>(string name, Type propertyType)
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(property => property.Name == name && property.PropertyType == propertyType)
            .GetMethod!;
    }

    [Fact]
    public void Print()
    {
        var methods = typeof(DerivedLeafCovariantReturnService).GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            var dt = method.DeclaringType;
            if (dt == typeof(object))
                continue;

            var attributes = method.GetCustomAttributesData();
            if (attributes.Any(m => m.AttributeType == typeof(CompilerGeneratedAttribute)))
                continue;

            var attributeNames = attributes.Select(a => a.AttributeType.Name);
            output.WriteLine($"[{dt?.Name}.{method.Name}] Return Type: {method.ReturnType.Name}, Attributes: {string.Join(", ", attributeNames)}");
        }
    }
}
