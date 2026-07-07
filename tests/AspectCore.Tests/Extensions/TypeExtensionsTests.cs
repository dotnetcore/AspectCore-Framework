using System.Linq;
using System;
using System.Reflection;
using AspectCore.Extensions;
using AspectCore.Tests;
using Xunit;

namespace AspectCore.Tests.Extensions;

public class TypeExtensionsTests
{
    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnTrue_WhenMethodIsOverriddenWithCovariantReturnType()
    {
        var method = GetMethod<CovariantReturnTypes.CommonService>(nameof(CovariantReturnTypes.CommonService.Method), typeof(object));
        var covariantReturnMethod = GetMethod<CovariantReturnTypes.BaseCovariantReturnService>(nameof(CovariantReturnTypes.BaseCovariantReturnService.Method), typeof(CovariantReturnTypes.BaseResult));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenMethodNamesDiffer()
    {
        var method = GetMethod<CovariantReturnTypes.CommonService>(nameof(CovariantReturnTypes.CommonService.Method), typeof(object));
        var covariantReturnMethod = GetPropertyGetter<CovariantReturnTypes.BaseCovariantReturnService>(nameof(CovariantReturnTypes.BaseCovariantReturnService.Property), typeof(CovariantReturnTypes.BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenReturnTypesAreTheSame()
    {
        var method = GetMethod<CovariantReturnTypes.BaseCovariantReturnService>(nameof(CovariantReturnTypes.BaseCovariantReturnService.Method), typeof(CovariantReturnTypes.BaseResult));
        var covariantReturnMethod = GetMethod<CovariantReturnTypes.BaseCovariantReturnService>(nameof(CovariantReturnTypes.BaseCovariantReturnService.Method), typeof(CovariantReturnTypes.BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenReturnTypeIsNotAssignable()
    {
        var method = GetMethod<CovariantReturnTypes.BaseCovariantReturnService>(nameof(CovariantReturnTypes.BaseCovariantReturnService.Method), typeof(CovariantReturnTypes.BaseResult));
        var covariantReturnMethod = GetMethod<CovariantReturnTypes.CommonService>(nameof(CovariantReturnTypes.CommonService.Method), typeof(object));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenParametersDiffer()
    {
        var method = GetMethod<CovariantReturnTypes.CommonService>(nameof(CovariantReturnTypes.CommonService.Method), typeof(object));
        var covariantReturnMethod = GetMethod<CovariantReturnTypes.GenericMethodLeafService>(nameof(CovariantReturnTypes.GenericMethodLeafService.Convert), typeof(CovariantReturnTypes.LeafResult), parameterCount: 1);

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnTrue_WhenGenericMethodIsOverriddenWithCovariantReturnType()
    {
        var method = GetMethod<CovariantReturnTypes.GenericMethodBaseService>(nameof(CovariantReturnTypes.GenericMethodBaseService.Convert), typeof(CovariantReturnTypes.BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<CovariantReturnTypes.GenericMethodLeafService>(nameof(CovariantReturnTypes.GenericMethodLeafService.Convert), typeof(CovariantReturnTypes.LeafResult), parameterCount: 1);

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    private static MethodInfo GetMethod<T>(string name, Type returnType, int parameterCount = 0)
    {
        return typeof(T).GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(method =>
                method.Name == name
                && method.ReturnType == returnType
                && method.GetParameters().Length == parameterCount);
    }

    private static MethodInfo GetPropertyGetter<T>(string name, Type propertyType)
    {
        return typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Single(property => property.Name == name && property.PropertyType == propertyType)
            .GetMethod!;
    }
}
