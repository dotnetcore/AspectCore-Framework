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

// define a partial class to hold the test types.
partial class CovariantReturnTypeTests
{
    public class ReturnTypeInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);

            if (context.ReturnValue is BaseResult returnValue)
            {
                returnValue.Name += nameof(ReturnTypeInterceptor);
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
        public virtual object Property { get; } = nameof(Property);
        public virtual object Method() => nameof(Method);

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(InterceptedProperty);
        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod() => nameof(InterceptedMethod);
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

    // this class just inherits from OrdinaryOverrideService, and does not override any members.
    public class DerivedOrdinaryOverrideService : OrdinaryOverrideService;
}

