using AspectCore.DynamicProxy;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Abstractions;

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

    private static void AssertCommonServiceReturns<T>(ICommonService service, string expectedName)
        where T : BaseResult
    {
        AssertTypeValue<T>(service.Property, v => Assert.Equal(expectedName, v.Name));
        AssertTypeValue<T>(service.Method(), v => Assert.Equal(expectedName, v.Name));
        AssertTypeValue<T>(service.InterceptedProperty, v => Assert.Equal(expectedName + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<T>(service.InterceptedMethod(), v => Assert.Equal(expectedName + nameof(ReturnTypeInterceptor), v.Name));
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

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantReturnTypeAndInterfaceView_ShouldUseLeafMethodAndProperty()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantReturnTypeAndBaseClassView_ShouldUseLeafMethodAndProperty()
    {
        var service = Assert.IsAssignableFrom<MidCovariantReturnService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedLeafCovariantReturnTypeAndInterfaceView_ShouldUseInheritedLeafMembers()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(ProxyGenerator.CreateClassProxy<DerivedLeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
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

        AssertCommonServiceReturns<MidResult>(service, nameof(MidCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndLeafImplementation_ShouldUseLeafInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, LeafCovariantReturnService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndLeafImplementationInterfaceView_ShouldUseLeafInterceptedMembers()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(
            ProxyGenerator.CreateClassProxy<CommonService, LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
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

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndLeafImplementationInterfaceView_ShouldUseLeafInterceptedMembers()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(
            ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
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

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndDerivedLeafImplementation_ShouldUseInheritedLeafMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, DerivedLeafCovariantReturnService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndOrdinaryOverrideImplementation_ShouldUseOrdinaryOverrideMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, OrdinaryOverrideService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(OrdinaryOverrideService));
    }

    [Fact]
    public void CreateClassProxy_ForMethodOnlyOrdinaryOverrideAfterCovariantReturnChain_ShouldUseOrdinaryOverrideMethod()
    {
        var service = ProxyGenerator.CreateClassProxy<MethodOnlyOrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(MethodOnlyOrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(MethodOnlyOrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForPropertyOnlyOrdinaryOverrideAfterCovariantReturnChain_ShouldUseOrdinaryOverrideProperty()
    {
        var service = ProxyGenerator.CreateClassProxy<PropertyOnlyOrdinaryOverrideService>();
        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(PropertyOnlyOrdinaryOverrideService), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(PropertyOnlyOrdinaryOverrideService) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForInvariantGenericReturnHiddenMembers_ShouldNotTreatMembersAsCovariantOverrides()
    {
        var service = ProxyGenerator.CreateClassProxy<InvariantGenericReturnLeafService>();

        Assert.IsType<List<LeafResult>>(service.Create());
        Assert.IsType<List<LeafResult>>(service.Items);
    }

    [Fact]
    public void CreateInterfaceProxy_ForCovariantInterface_ShouldUseLeafInterfaceMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICovariantInterfaceLeafService, CovariantInterfaceLeafImplementation>();

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseCovariantInterfaceAndLeafImplementation_ShouldUseLeafImplementationMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICovariantInterfaceBaseService, CovariantInterfaceLeafImplementation>();

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.Method(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod(), v => Assert.Equal(nameof(CovariantInterfaceLeafImplementation) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForIndexerCovariantReturn_ShouldUseLeafIndexerGetter()
    {
        var service = ProxyGenerator.CreateClassProxy<IndexerLeafCovariantReturnService>();

        AssertTypeValue<LeafResult>(service[0], v => Assert.Equal(nameof(IndexerLeafCovariantReturnService), v.Name));
        AssertTypeValue<LeafResult>(service["key"], v => Assert.Equal(nameof(IndexerLeafCovariantReturnService) + nameof(ReturnTypeInterceptor), v.Name));
    }
}
