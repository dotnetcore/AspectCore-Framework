using System.Linq;
using System;
using System.Reflection;
using AspectCore.Extensions;
using Xunit;
using static AspectCore.Tests.CovariantReturnTypes;

namespace AspectCore.Tests.Extensions;

public class TypeExtensionsTests
{
    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnTrue_WhenMethodIsOverriddenWithCovariantReturnType()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantReturnMethod = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.True(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenMethodNamesDiffer()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantReturnMethod = GetPropertyGetter<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Property), typeof(BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenReturnTypesAreTheSame()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));
        var covariantReturnMethod = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenReturnTypeIsNotAssignable()
    {
        var method = GetMethod<BaseCovariantReturnService>(nameof(BaseCovariantReturnService.Method), typeof(BaseResult));
        var covariantReturnMethod = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnFalse_WhenParametersDiffer()
    {
        var method = GetMethod<CommonService>(nameof(CommonService.Method), typeof(object));
        var covariantReturnMethod = GetMethod<GenericMethodLeafService>(nameof(GenericMethodLeafService.Convert), typeof(LeafResult), parameterCount: 1);

        Assert.False(method.IsOverriddenByCovariantReturnMethod(covariantReturnMethod));
    }

    [Fact]
    public void IsOverriddenByCovariantReturnMethod_ShouldReturnTrue_WhenGenericMethodIsOverriddenWithCovariantReturnType()
    {
        var method = GetMethod<GenericMethodBaseService>(nameof(GenericMethodBaseService.Convert), typeof(BaseResult), parameterCount: 1);
        var covariantReturnMethod = GetMethod<GenericMethodLeafService>(nameof(GenericMethodLeafService.Convert), typeof(LeafResult), parameterCount: 1);

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
