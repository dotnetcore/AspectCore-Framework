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
    public void CreateClassProxy_ForLeafCovariantReturnTypeAndInterfaceMethodView_ShouldUseLeafInterceptedMethod()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertTypeValue<string>(service.Method(), v => Assert.Equal(nameof(CommonService), v));
        AssertTypeValue<string>(service.InterceptedMethod(), v => Assert.Equal(nameof(CommonService) + nameof(ReturnTypeInterceptor), v));
    }

    [Fact]
    public void CreateClassProxy_ForLeafCovariantReturnTypeAndInterfacePropertyView_ShouldUseLeafInterceptedProperty()
    {
        var service = Assert.IsAssignableFrom<ICommonService>(ProxyGenerator.CreateClassProxy<LeafCovariantReturnService>());

        AssertTypeValue<string>(service.Property, v => Assert.Equal(nameof(CommonService), v));
        AssertTypeValue<string>(service.InterceptedProperty, v => Assert.Equal(nameof(CommonService) + nameof(ReturnTypeInterceptor), v));
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

        Assert.IsType<System.Collections.Generic.List<LeafResult>>(service.Create());
        Assert.IsType<System.Collections.Generic.List<LeafResult>>(service.Items);
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

// a partial class is used here to separate the test classes from the test methods, for better organization.
partial class CovariantReturnTypeTests
{
    public class ReturnTypeInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);

            switch (context.ReturnValue)
            {
                case BaseResult returnValue:
                {
                    returnValue.Name += nameof(ReturnTypeInterceptor);
                    break;
                }
                case string str:
                {
                    context.ReturnValue = str + nameof(ReturnTypeInterceptor);
                    break;
                }
            }
        }
    }

    public interface ICommonService
    {
        object Property { get; }
        object Method();

        object InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        object InterceptedMethod();
    }

    public class CommonService : ICommonService
    {
        public virtual object Property { get; } = nameof(CommonService);
        public virtual object Method() => nameof(CommonService);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(CommonService);
        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod() => nameof(CommonService);
    }

    public class BaseCovariantReturnService : CommonService
    {
        public override BaseResult Property { get; } = new(nameof(BaseCovariantReturnService));
        public override BaseResult Method() => new(nameof(BaseCovariantReturnService));

        public override BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(BaseCovariantReturnService));
        [ReturnTypeInterceptor]
        public override BaseResult InterceptedMethod() => new(nameof(BaseCovariantReturnService));
    }

    public class MidCovariantReturnService : BaseCovariantReturnService
    {
        public override MidResult Property { get; } = new(nameof(MidCovariantReturnService));
        public override MidResult Method() => new(nameof(MidCovariantReturnService));

        public override MidResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(MidCovariantReturnService));
        [ReturnTypeInterceptor]
        public override MidResult InterceptedMethod() => new(nameof(MidCovariantReturnService));
    }

    public class LeafCovariantReturnService : MidCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(LeafCovariantReturnService));
        public override LeafResult Method() => new(nameof(LeafCovariantReturnService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(LeafCovariantReturnService));
        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(LeafCovariantReturnService));
    }

    // this class just does ordinary overriding.
    public class OrdinaryOverrideService : LeafCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(OrdinaryOverrideService));
        public override LeafResult Method() => new(nameof(OrdinaryOverrideService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(OrdinaryOverrideService));
        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(OrdinaryOverrideService));
    }

    //this class just inherits from OrdinaryOverrideService, and does not override any members.
    public class DerivedOrdinaryOverrideService : OrdinaryOverrideService;

    public class MethodOnlyCommonService
    {
        public virtual object Method() => nameof(Method);

        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod() => nameof(InterceptedMethod);
    }

    public class MethodOnlyBaseCovariantReturnService : MethodOnlyCommonService
    {
        public override BaseResult Method() => new(nameof(MethodOnlyBaseCovariantReturnService));

        [ReturnTypeInterceptor]
        public override BaseResult InterceptedMethod() => new(nameof(MethodOnlyBaseCovariantReturnService));
    }

    public class MethodOnlyLeafCovariantReturnService : MethodOnlyBaseCovariantReturnService
    {
        public override LeafResult Method() => new(nameof(MethodOnlyLeafCovariantReturnService));

        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(MethodOnlyLeafCovariantReturnService));
    }

    public class MethodOnlyOrdinaryOverrideService : MethodOnlyLeafCovariantReturnService
    {
        public override LeafResult Method() => new(nameof(MethodOnlyOrdinaryOverrideService));

        [ReturnTypeInterceptor]
        public override LeafResult InterceptedMethod() => new(nameof(MethodOnlyOrdinaryOverrideService));
    }

    public class PropertyOnlyCommonService
    {
        public virtual object Property { get; } = nameof(Property);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(InterceptedProperty);
    }

    public class PropertyOnlyBaseCovariantReturnService : PropertyOnlyCommonService
    {
        public override BaseResult Property { get; } = new(nameof(PropertyOnlyBaseCovariantReturnService));

        public override BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(PropertyOnlyBaseCovariantReturnService));
    }

    public class PropertyOnlyLeafCovariantReturnService : PropertyOnlyBaseCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(PropertyOnlyLeafCovariantReturnService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(PropertyOnlyLeafCovariantReturnService));
    }

    public class PropertyOnlyOrdinaryOverrideService : PropertyOnlyLeafCovariantReturnService
    {
        public override LeafResult Property { get; } = new(nameof(PropertyOnlyOrdinaryOverrideService));

        public override LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(PropertyOnlyOrdinaryOverrideService));
    }

    public class InvariantGenericReturnBaseService
    {
        public virtual System.Collections.Generic.List<BaseResult> Items { get; } = [];

        public virtual System.Collections.Generic.List<BaseResult> Create() => [];
    }

    public class InvariantGenericReturnLeafService : InvariantGenericReturnBaseService
    {
        public new System.Collections.Generic.List<LeafResult> Items { get; } = [];

        public new System.Collections.Generic.List<LeafResult> Create() => [];
    }

    public interface ICovariantInterfaceBaseService
    {
        BaseResult Property { get; }
        BaseResult Method();

        BaseResult InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        BaseResult InterceptedMethod();
    }

    public interface ICovariantInterfaceLeafService : ICovariantInterfaceBaseService
    {
        new LeafResult Property { get; }
        new LeafResult Method();

        new LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        new LeafResult InterceptedMethod();
    }

    public class CovariantInterfaceLeafImplementation : ICovariantInterfaceLeafService
    {
        public LeafResult Property { get; } = new(nameof(CovariantInterfaceLeafImplementation));
        public LeafResult Method() => new(nameof(CovariantInterfaceLeafImplementation));

        public LeafResult InterceptedProperty { [ReturnTypeInterceptor] get; } = new(nameof(CovariantInterfaceLeafImplementation));
        [ReturnTypeInterceptor]
        public LeafResult InterceptedMethod() => new(nameof(CovariantInterfaceLeafImplementation));

        BaseResult ICovariantInterfaceBaseService.Property => Property;
        BaseResult ICovariantInterfaceBaseService.Method() => Method();

        BaseResult ICovariantInterfaceBaseService.InterceptedProperty => InterceptedProperty;
        BaseResult ICovariantInterfaceBaseService.InterceptedMethod() => InterceptedMethod();
    }

    public class IndexerCommonService
    {
        public virtual object this[int index] => nameof(IndexerCommonService);

        public virtual object this[string key] { [ReturnTypeInterceptor] get => nameof(IndexerCommonService); }
    }

    public class IndexerBaseCovariantReturnService : IndexerCommonService
    {
        public override BaseResult this[int index] => new(nameof(IndexerBaseCovariantReturnService));

        public override BaseResult this[string key] { [ReturnTypeInterceptor] get => new(nameof(IndexerBaseCovariantReturnService)); }
    }

    public class IndexerLeafCovariantReturnService : IndexerBaseCovariantReturnService
    {
        public override LeafResult this[int index] => new(nameof(IndexerLeafCovariantReturnService));

        public override LeafResult this[string key] { [ReturnTypeInterceptor] get => new(nameof(IndexerLeafCovariantReturnService)); }
    }
}
