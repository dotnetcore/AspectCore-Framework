#pragma warning disable IDE0060 // Remove unused parameter
using AspectCore.DynamicProxy;
using System.Collections.Generic;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public partial class CovariantReturnTypeTests
{
    [Fact]
    public void CreateClassProxy_ForGenericMethodCovariantReturn_ShouldUseLeafMethod()
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
    public void CreateClassProxy_ForGenericMethodShapeCovariantReturn_ShouldMatchMethodGenericParameterShapes()
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
    public void CreateClassProxy_ForGenericMethodReturnTypeContainingMethodGenericParameter_ShouldUseLeafReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericMethodShapeLeafService>();

        Assert.IsType<List<string>>(service.ReturnList<string>());
    }

    [Fact]
    public void CreateClassProxy_ForGenericTypeCovariantReturn_ShouldUseLeafMethodAndProperty()
    {
        var service = ProxyGenerator.CreateClassProxy<TypeGenericShapeLeafService<string>>();

        AssertTypeValue<LeafResult>(service.Direct("value"), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.List(["value"]), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        Assert.IsType<List<string>>(service.ReturnList);
    }

    [Fact]
    public void CreateClassProxy_ForGenericLeafCovariantReturnType_ShouldUseLeafMethodAndProperty()
    {
        var service = ProxyGenerator.CreateClassProxy<GenericLeafCovariantReturnService<string>>();

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.Method("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForGenericLeafCovariantReturnTypeAndInterfaceView_ShouldUseLeafMethodAndProperty()
    {
        var service = Assert.IsAssignableFrom<IGenericCommonService<string>>(
            ProxyGenerator.CreateClassProxy<GenericLeafCovariantReturnService<string>>());

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.Method("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForGenericLeafCovariantReturnTypeAndBaseClassView_ShouldUseLeafMethodAndProperty()
    {
        var service = Assert.IsAssignableFrom<GenericCovariantReturnService<string>>(
            ProxyGenerator.CreateClassProxy<GenericLeafCovariantReturnService<string>>());

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.Method("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateClassProxy_ForGenericTypeBaseServiceAndLeafImplementation_ShouldUseLeafMethodAndProperty()
    {
        var service = ProxyGenerator.CreateClassProxy<TypeGenericShapeBaseService<string>, TypeGenericShapeLeafService<string>>();

        AssertTypeValue<LeafResult>(service.Direct("value"), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.List(["value"]), v => Assert.Equal(nameof(TypeGenericShapeLeafService<string>), v.Name));
        Assert.IsType<List<string>>(service.ReturnList);
    }

    [Fact]
    public void CreateInterfaceProxy_ForGenericInterfaceAndCovariantImplementation_ShouldUseBaseResultMethodAndProperty()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IGenericCommonService<string>, GenericCovariantReturnService<string>>();

        AssertTypeValue<BaseResult>(service.Property, v => Assert.Equal(nameof(GenericCovariantReturnService<string>), v.Name));
        AssertTypeValue<BaseResult>(service.Method("value"), v => Assert.Equal(nameof(GenericCovariantReturnService<string>), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedProperty, v => Assert.Equal(nameof(GenericCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<BaseResult>(service.InterceptedMethod("value"), v => Assert.Equal(nameof(GenericCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
    }

    [Fact]
    public void CreateInterfaceProxy_ForGenericInterfaceAndLeafImplementation_ShouldUseLeafMethodAndProperty()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IGenericCommonService<string>, GenericLeafCovariantReturnService<string>>();

        AssertTypeValue<LeafResult>(service.Property, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.Method("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedProperty, v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
        AssertTypeValue<LeafResult>(service.InterceptedMethod("value"), v => Assert.Equal(nameof(GenericLeafCovariantReturnService<string>) + nameof(ReturnTypeInterceptor), v.Name));
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
    public void CreateClassProxy_ForConstrainedGenericCovariantReturn_ShouldPreserveGenericReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<ConstrainedGenericReturnLeafService>();
        var value = new LeafResult("leaf");

        Assert.Same(value, service.Create(value));
    }

}
