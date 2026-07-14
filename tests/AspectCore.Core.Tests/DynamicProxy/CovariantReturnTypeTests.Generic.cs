#pragma warning disable IDE0060 // Remove unused parameter
using AspectCore.DynamicProxy;
using System.Collections.Generic;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy;

public partial class CovariantReturnTypeTests
{
    private static void AssertGenericCommonServiceReturns<T>(IGenericCommonService<string> service, string expectedName)
        where T : BaseResult
    {
        AssertTypeValue<T>(service.Property, v => Assert.Equal(expectedName, v.Name));
        AssertTypeValue<T>(service.Method("value"), v => Assert.Equal(expectedName, v.Name));
        AssertTypeValue<T>(service.InterceptedProperty, v => Assert.Equal(expectedName + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<T>(service.InterceptedMethod("value"), v => Assert.Equal(expectedName + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForGenericMethodCovariantOverride_ShouldUseLeafMethod()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericMethodLeafService>();

        AssertTypeValue<LeafResult>(service.Convert("value"), v => Assert.Equal(nameof(GenericMethodLeafService), v.Name));
        AssertTypeValue<LeafResult>(service.Convert(1), v => Assert.Equal(nameof(GenericMethodLeafService), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForGenericMethodBaseServiceAndLeafImplementation_ShouldUseLeafMethod()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericMethodBaseService, GenericMethodLeafService>();

        AssertTypeValue<LeafResult>(service.Convert("value"), v => Assert.Equal(nameof(GenericMethodLeafService), v.Name));
        AssertTypeValue<LeafResult>(service.Convert(1), v => Assert.Equal(nameof(GenericMethodLeafService), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForGenericMethodParameterShapes_ShouldUseLeafMethods()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericMethodShapeLeafService>();
        var byRefValue = "value";

        AssertTypeValue<LeafResult>(service.Direct("value"), v => Assert.Equal(nameof(GenericMethodShapeLeafService), v.Name));
        AssertTypeValue<LeafResult>(service.Array(["value"]), v => Assert.Equal(nameof(GenericMethodShapeLeafService), v.Name));
        AssertTypeValue<LeafResult>(service.List(["value"]), v => Assert.Equal(nameof(GenericMethodShapeLeafService), v.Name));
        AssertTypeValue<LeafResult>(service.Dictionary(new Dictionary<string, string> { ["key"] = "value" }), v => Assert.Equal(nameof(GenericMethodShapeLeafService), v.Name));
        AssertTypeValue<LeafResult>(service.ByRef(ref byRefValue), v => Assert.Equal(nameof(GenericMethodShapeLeafService), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForGenericMethodReturnContainingMethodParameter_ShouldUseLeafReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericMethodShapeLeafService>();

        Assert.IsType<List<string>>(service.ReturnList<string>());
    }

    [Fact]
    public void CreateClassProxy_ForTypeGenericCovariantOverride_ShouldUseLeafMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<TypeGenericShapeLeafService<string>>();

        AssertTypeValue<LeafResult>(service.Direct("value"), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.List(["value"]), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        Assert.IsType<List<string>>(service.ReturnList);
    }

    [Fact]
    public void CreateClassProxy_ForGenericLeafCovariantOverride_ShouldUseLeafMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericLeafCovariantReturnService<string>>();

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateClassProxy_ForGenericLeafCovariantOverrideAsInterface_ShouldUseLeafMembers()
    {
        var service = Assert.IsAssignableFrom<IGenericCommonService<string>>(
            ProxyGenerator.CreateClassProxy<GenericLeafCovariantReturnService<string>>());

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateClassProxy_ForGenericLeafCovariantOverrideAsBaseClass_ShouldUseLeafMembers()
    {
        var service = Assert.IsAssignableFrom<GenericCovariantReturnService<string>>(
            ProxyGenerator.CreateClassProxy<GenericLeafCovariantReturnService<string>>());

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateClassProxy_ForGenericDerivedLeafOverrideAsInterface_ShouldUseInheritedLeafMembers()
    {
        var service = Assert.IsAssignableFrom<IGenericCommonService<string>>(
            ProxyGenerator.CreateClassProxy<GenericDerivedLeafCovariantReturnService<string>>());

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateClassProxy_ForTypeGenericBaseServiceAndLeafImplementation_ShouldUseLeafMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<TypeGenericShapeBaseService<string>, TypeGenericShapeLeafService<string>>();

        AssertTypeValue<LeafResult>(service.Direct("value"), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.List(["value"]), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        Assert.IsType<List<string>>(service.ReturnList);
    }

    [Fact]
    public void CreateClassProxy_ForGenericBaseServiceAndLeafImplementation_ShouldUseLeafMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericCommonService<string>, GenericLeafCovariantReturnService<string>>();

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateClassProxy_ForGenericBaseServiceAndLeafImplementationAsInterface_ShouldUseLeafMembers()
    {
        var service = Assert.IsAssignableFrom<IGenericCommonService<string>>(
            ProxyGenerator.CreateClassProxy<GenericCommonService<string>, GenericLeafCovariantReturnService<string>>());

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateClassProxy_ForGenericCovariantServiceAndLeafImplementationAsInterface_ShouldUseLeafMembers()
    {
        var service = Assert.IsAssignableFrom<IGenericCommonService<string>>(
            ProxyGenerator.CreateClassProxy<GenericCovariantReturnService<string>, GenericLeafCovariantReturnService<string>>());

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateInterfaceProxy_ForGenericInterfaceAndFirstCovariantImplementation_ShouldUseBaseResultMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IGenericCommonService<string>, GenericCovariantReturnService<string>>();

        AssertGenericCommonServiceReturns<BaseResult>(service, nameof(GenericCovariantReturnService<string>));
    }

    [Fact]
    public void CreateInterfaceProxy_ForGenericInterfaceAndLeafImplementation_ShouldUseLeafMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IGenericCommonService<string>, GenericLeafCovariantReturnService<string>>();

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateInterfaceProxy_ForGenericInterfaceAndDerivedLeafImplementation_ShouldUseInheritedLeafMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IGenericCommonService<string>, GenericDerivedLeafCovariantReturnService<string>>();

        AssertGenericCommonServiceReturns<LeafResult>(service, nameof(GenericLeafCovariantReturnService<string>));
    }

    [Fact]
    public void CreateClassProxy_ForClosedBaseGenericParameter_ShouldNotBindUnrelatedLeafGenericParameter()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericParameterSubstitutionLeafService<string>>();
        var baseService = Assert.IsAssignableFrom<GenericParameterSubstitutionBaseService<BaseResult>>(service);

        AssertTypeValue<BaseResult>(baseService.Convert(new BaseResult("base")), v => Assert.Equal(nameof(GenericParameterSubstitutionBaseService<BaseResult>), v.Name));
        AssertTypeValue<LeafResult>(service.Convert("leaf"), v => Assert.Equal(nameof(GenericParameterSubstitutionLeafService<string>), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForConstrainedGenericMethodCovariantReturn_ShouldPreserveGenericReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<ConstrainedGenericReturnLeafService>();
        var value = new LeafResult("leaf");

        Assert.Same(value, service.Create(value));
    }

}
