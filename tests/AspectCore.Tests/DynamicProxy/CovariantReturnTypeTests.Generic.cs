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
    public void CreateClassProxy_ForGenericCommonCovariantReturnTypeAndInterfaceView_ShouldUseLeafMethodAndProperty()
    {
        // CreateClassProxy won't create a proxy for the interface
        var service = Assert.IsAssignableFrom<IGenericCommonService<string>>(
            ProxyGenerator.CreateClassProxy<GenericCommonService<string>>());

        AssertTypeValue<string>(service.Property, v => Assert.Equal(nameof(GenericCommonService<string>), v));
        AssertTypeValue<string>(service.Method("value"), v => Assert.Equal(nameof(GenericCommonService<string>), v));
        AssertTypeValue<string>(service.InterceptedProperty, v => Assert.Equal(nameof(GenericCommonService<string>) + nameof(ReturnTypeInterceptor), v));
        AssertTypeValue<string>(service.InterceptedMethod("value"), v => Assert.Equal(nameof(GenericCommonService<string>) + nameof(ReturnTypeInterceptor), v));
    }

    [Fact]
    public void CreateClassProxy_ForGenericLeafCovariantReturnTypeAndInterfaceView_ShouldUseLeafMethodAndProperty()
    {
        var service = Assert.IsAssignableFrom<IGenericCommonService<string>>(
            ProxyGenerator.CreateClassProxy<GenericLeafCovariantReturnService<string>>());

        // CreateClassProxy won't create a proxy for the interface
        AssertTypeValue<string>(service.Property, v => Assert.Equal(nameof(GenericCommonService<string>), v));
        AssertTypeValue<string>(service.Method("value"), v => Assert.Equal(nameof(GenericCommonService<string>), v));
        AssertTypeValue<string>(service.InterceptedProperty, v => Assert.Equal(nameof(GenericCommonService<string>) + nameof(ReturnTypeInterceptor), v));
        AssertTypeValue<string>(service.InterceptedMethod("value"), v => Assert.Equal(nameof(GenericCommonService<string>) + nameof(ReturnTypeInterceptor), v));
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

// a partial class is used here to separate the test classes from the test methods, for better organization.
partial class CovariantReturnTypeTests
{
    public class GenericMethodBaseService
    {
        public virtual BaseResult Convert<TValue>(TValue value) => new(nameof(GenericMethodBaseService));
    }

    public class GenericMethodLeafService : GenericMethodBaseService
    {
        public override LeafResult Convert<TValue>(TValue value) => new(nameof(GenericMethodLeafService));
    }

    public class GenericMethodShapeBaseService
    {
        public virtual BaseResult Direct<TValue>(TValue value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult Array<TValue>(TValue[] value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult List<TValue>(List<TValue> value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult Dictionary<TValue>(Dictionary<string, TValue> value) => new(nameof(GenericMethodShapeBaseService));

        public virtual BaseResult ByRef<TValue>(ref TValue value) => new(nameof(GenericMethodShapeBaseService));

        public virtual IEnumerable<TValue> ReturnList<TValue>() => [];
    }

    public class GenericMethodShapeLeafService : GenericMethodShapeBaseService
    {
        public override LeafResult Direct<TValue>(TValue value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult Array<TValue>(TValue[] value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult List<TValue>(List<TValue> value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult Dictionary<TValue>(Dictionary<string, TValue> value) => new(nameof(GenericMethodShapeLeafService));

        public override LeafResult ByRef<TValue>(ref TValue value) => new(nameof(GenericMethodShapeLeafService));

        public override List<TValue> ReturnList<TValue>() => [];
    }

    public class TypeGenericShapeBaseService<TValue>
    {
        public virtual BaseResult Direct(TValue value) => new(nameof(TypeGenericShapeBaseService<TValue>));

        public virtual BaseResult List(List<TValue> value) => new(nameof(TypeGenericShapeBaseService<TValue>));

        public virtual IEnumerable<TValue> ReturnList => [];
    }

    public class TypeGenericShapeLeafService<TValue> : TypeGenericShapeBaseService<TValue>
    {
        public override LeafResult Direct(TValue value) => new(nameof(TypeGenericShapeLeafService<TValue>));

        public override LeafResult List(List<TValue> value) => new(nameof(TypeGenericShapeLeafService<TValue>));

        public override List<TValue> ReturnList { get; } = [];
    }

    public interface IGenericCommonService<in TValue>
    {
        object Property { get; }
        object Method(TValue value);

        object InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        object InterceptedMethod(TValue value);
    }

    public class GenericCommonService<TValue> : IGenericCommonService<TValue>
    {
        public virtual object Property { get; } = nameof(GenericCommonService<TValue>);
        public virtual object Method(TValue value) => nameof(GenericCommonService<TValue>);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(GenericCommonService<TValue>);
        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod(TValue value) => nameof(GenericCommonService<TValue>);
    }

    public class GenericCovariantReturnService<TValue> : GenericCommonService<TValue>
    {
        public override BaseResult Property { get; } = new(nameof(GenericCovariantReturnService<TValue>));
        public override BaseResult Method(TValue value) => new(nameof(GenericCovariantReturnService<TValue>));

        public override BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(GenericCovariantReturnService<TValue>));
        [ReturnTypeInterceptor]
        public override BaseResult InterceptedMethod(TValue value) => new(nameof(GenericCovariantReturnService<TValue>));
    }

    public class GenericLeafCovariantReturnService<TValue> : GenericCovariantReturnService<TValue>
    {
        public override LeafResult Property { get; } = new(nameof(GenericLeafCovariantReturnService<TValue>));
        public override LeafResult Method(TValue value) => new(nameof(GenericLeafCovariantReturnService<TValue>));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(GenericLeafCovariantReturnService<TValue>));
        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod(TValue value) => new(nameof(GenericLeafCovariantReturnService<TValue>));
    }

    public class GenericParameterSubstitutionBaseService<TBase>
    {
        public virtual BaseResult Convert(TBase value) => new(nameof(GenericParameterSubstitutionBaseService<TBase>));
    }

    public class GenericParameterSubstitutionLeafService<TLeaf> : GenericParameterSubstitutionBaseService<BaseResult>
    {
        public LeafResult Convert(TLeaf value) => new(nameof(GenericParameterSubstitutionLeafService<TLeaf>));
    }

    public class ConstrainedGenericReturnBaseService
    {
        public virtual BaseResult Create<TValue>(TValue value) where TValue : LeafResult
            => value;
    }

    public class ConstrainedGenericReturnLeafService : ConstrainedGenericReturnBaseService
    {
        public override TValue Create<TValue>(TValue value) => value;
    }
}
