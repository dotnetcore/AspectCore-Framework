using System;
using AspectCore.DynamicProxy;
using Xunit;
using static AspectCore.Tests.CovariantReturnTypes;

namespace AspectCore.Tests.DynamicProxy;

public class CovariantReturnMethodTests : DynamicProxyTestBase
{
    /// <summary>
    /// Verifies that an object is exactly the given type (and not a derived type), and that it satisfies the given predicate.
    /// </summary>
    private static void AssertTypeValue<T>(object value, Func<T, bool> predicate)
    {
        var v = Assert.IsType<T>(value);
        Assert.True(predicate(v));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantReturnType_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Method(), v => v.Name == nameof(BaseCovariantReturnService));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => v.Name == nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor));
        AssertTypeValue<BaseResult>(service.Property, v => v.Name == nameof(BaseCovariantReturnService));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => v.Name == nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedCovariantReturnType_ShouldUseOverriddenInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.Method(), v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Property, v => v.Name == nameof(BaseCovariantReturnService));
        AssertTypeValue<BaseResult>(service.Method(), v => v.Name == nameof(BaseCovariantReturnService));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => v.Name == nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => v.Name == nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.Method(), v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Method(), v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
        AssertTypeValue<MidResult>(service.Property, v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Property, v => v.Name == nameof(BaseCovariantReturnService));
        AssertTypeValue<BaseResult>(service.Method(), v => v.Name == nameof(BaseCovariantReturnService));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => v.Name == nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => v.Name == nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.Method(), v => v.Name == nameof(MidCovariantReturnService));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => v.Name == nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor));
    }
}
