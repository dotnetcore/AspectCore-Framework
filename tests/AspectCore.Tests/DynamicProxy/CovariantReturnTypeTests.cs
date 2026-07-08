using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public partial class CovariantReturnTypeTests : DynamicProxyTestBase
{
    /// <summary>
    /// Verifies that an object is exactly the given type (and not a derived type), and that it satisfies the given predicate.
    /// </summary>
    private static void AssertTypeValue<T>(object value, Action<T> action)
    {
        var v = Assert.IsType<T>(value);
        action(v);
    }

    [Fact]
    public void CreateClassProxy_ForCovariantReturnType_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Method(), v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<BaseResult>(service.Property, v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedCovariantReturnType_ShouldUseOverriddenInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantReturnType_ShouldUseLeafInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForOrdinaryOverrideAfterCovariantReturnChain_ShouldUseOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<OrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedOrdinaryOverrideAfterCovariantReturnChain_ShouldUseInheritedOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<DerivedOrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Property, v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.Method(), v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndLeafImplementation_ShouldUseLeafInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, LeafCovariantReturnService>();
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForLeafServiceAndOrdinaryOverrideImplementation_ShouldUseOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<LeafCovariantReturnService, OrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, BaseCovariantReturnService>();
        AssertTypeValue<BaseResult>(service.Property, v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.Method(), v => Assert.Equal(nameof(BaseCovariantReturnService), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(BaseCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, MidCovariantReturnService>();
        AssertTypeValue<MidResult>(service.Property, v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.Method(), v => Assert.Equal(nameof(MidCovariantReturnService), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedProperty, v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<MidResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MidCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndLeafImplementation_ShouldUseLeafInterceptedMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, LeafCovariantReturnService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(LeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(LeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndOrdinaryOverrideImplementation_ShouldUseOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, OrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(OrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(OrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }
}

