using AspectCore.DynamicProxy;
using System;
using System.Collections.Generic;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy;

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
    public void CreateClassProxy_ForFirstCovariantOverride_ShouldUseBaseResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService>();

        AssertCommonServiceReturns<BaseResult>(service, nameof(BaseCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForMidCovariantOverride_ShouldUseMidResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<MidCovariantReturnService>();

        AssertCommonServiceReturns<MidResult>(service, nameof(MidCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantOverride_ShouldUseLeafResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantOverrideAsInterface_ShouldUseLeafResultMembers()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantOverrideAsBaseClass_ShouldUseLeafResultMembers()
    {
        var service = Assert.IsAssignableFrom<MidCovariantReturnService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedLeafOverrideAsInterface_ShouldUseInheritedLeafResultMembers()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(ProxyGenerator.CreateClassProxy<DerivedLeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForOrdinaryOverrideAfterCovariantReturnChain_ShouldUseOrdinaryOverrideResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<OrdinaryOverrideService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(OrdinaryOverrideService));
    }

    [Fact]
    public void CreateClassProxy_ForDerivedOrdinaryOverrideAfterCovariantReturnChain_ShouldUseInheritedOrdinaryOverrideResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<DerivedOrdinaryOverrideService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(OrdinaryOverrideService));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndFirstCovariantImplementation_ShouldUseBaseResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, BaseCovariantReturnService>();

        AssertCommonServiceReturns<BaseResult>(service, nameof(BaseCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndMidImplementation_ShouldUseMidResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, MidCovariantReturnService>();

        AssertCommonServiceReturns<MidResult>(service, nameof(MidCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndLeafImplementation_ShouldUseLeafResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<CommonService, LeafCovariantReturnService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndLeafImplementationAsInterface_ShouldUseLeafResultMembers()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(
            ProxyGenerator.CreateClassProxy<CommonService, LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndMidImplementation_ShouldUseMidResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, MidCovariantReturnService>();

        AssertCommonServiceReturns<MidResult>(service, nameof(MidCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndLeafImplementation_ShouldUseLeafResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, LeafCovariantReturnService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndLeafImplementationAsInterface_ShouldUseLeafResultMembers()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(
            ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, LeafCovariantReturnService>());

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateClassProxy_ForLeafServiceAndOrdinaryOverrideImplementation_ShouldUseOrdinaryOverrideResultMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<LeafCovariantReturnService, OrdinaryOverrideService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(OrdinaryOverrideService));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndFirstCovariantImplementation_ShouldUseBaseResultMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, BaseCovariantReturnService>();

        AssertCommonServiceReturns<BaseResult>(service, nameof(BaseCovariantReturnService));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndMidImplementation_ShouldUseMidResultMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, MidCovariantReturnService>();

        AssertCommonServiceReturns<MidResult>(service, nameof(MidCovariantReturnService));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndLeafImplementation_ShouldUseLeafResultMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, LeafCovariantReturnService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndDerivedLeafImplementation_ShouldUseInheritedLeafResultMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICommonService, DerivedLeafCovariantReturnService>();

        AssertCommonServiceReturns<LeafResult>(service, nameof(LeafCovariantReturnService));
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndOrdinaryOverrideImplementation_ShouldUseOrdinaryOverrideResultMembers()
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
