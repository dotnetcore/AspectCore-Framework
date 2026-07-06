using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public class CovariantReturnMethodTests : DynamicProxyTestBase
{
    public class ReturnTypeInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);

            var returnType = context.ImplementationMethod.ReturnType;
            if (returnType == typeof(string))
            {
                context.ReturnValue += nameof(ReturnTypeInterceptor);
            }
            else if (returnType == typeof(object))
            {
                context.ReturnValue = nameof(ReturnTypeInterceptor);
            }
        }
    }

    public interface ICovariantReturnService
    {
        object Property { get; }
        object Method();

        object InterceptedProperty { [ReturnTypeInterceptor] get; }
        [ReturnTypeInterceptor]
        object InterceptedMethod();
    }

    public class BaseCovariantReturnService : ICovariantReturnService
    {
        public virtual object Property { get; } = 1;
        public virtual object Method() => 1;

        public virtual object InterceptedProperty { [ReturnTypeInterceptor] get; } = new();
        [ReturnTypeInterceptor]
        public virtual object InterceptedMethod() => new();
    }

    public class StringCovariantReturnService : BaseCovariantReturnService
    {
        public override string Property { get; } = nameof(StringCovariantReturnService);
        public override string Method() => nameof(StringCovariantReturnService);

        public override string InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(StringCovariantReturnService);
        [ReturnTypeInterceptor]
        public override string InterceptedMethod() => nameof(StringCovariantReturnService);
    }

    public class DerivedStringCovariantReturnService : StringCovariantReturnService
    {
        public override string InterceptedProperty { [ReturnTypeInterceptor] get; } = nameof(DerivedStringCovariantReturnService);
        [ReturnTypeInterceptor]
        public override string InterceptedMethod() => nameof(DerivedStringCovariantReturnService);
    }

    [Fact]
    public void CreateClassProxy_ForCovariantReturnType_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<StringCovariantReturnService>();
        Assert.Equal(nameof(StringCovariantReturnService), service.Method());
        Assert.Equal(nameof(StringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedMethod());
        Assert.Equal(nameof(StringCovariantReturnService), service.Property);
        Assert.Equal(nameof(StringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedProperty);
    }

    [Fact]
    public void CreateClassProxy_ForDerivedCovariantReturnType_ShouldUseOverriddenInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<DerivedStringCovariantReturnService>();
        Assert.Equal(nameof(StringCovariantReturnService), service.Property);
        Assert.Equal(nameof(StringCovariantReturnService), service.Method());
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedProperty);
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedMethod());
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, StringCovariantReturnService>();
        Assert.Equal(nameof(StringCovariantReturnService), service.Method());
        Assert.Equal(nameof(StringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedMethod());
        Assert.Equal(nameof(StringCovariantReturnService), service.Property);
        Assert.Equal(nameof(StringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedProperty);
    }

    [Fact]
    public void CreateClassProxy_ForBaseServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseCovariantReturnService, DerivedStringCovariantReturnService>();
        Assert.Equal(nameof(StringCovariantReturnService), service.Method());
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedMethod());
        Assert.Equal(nameof(StringCovariantReturnService), service.Property);
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedProperty);
    }

    [Fact]
    public void CreateClassProxy_ForCovariantServiceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateClassProxy<StringCovariantReturnService, DerivedStringCovariantReturnService>();
        Assert.Equal(nameof(StringCovariantReturnService), service.Method());
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedMethod());
        Assert.Equal(nameof(StringCovariantReturnService), service.Property);
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedProperty);
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndCovariantImplementation_ShouldUseStringReturnType()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICovariantReturnService, StringCovariantReturnService>();
        Assert.Equal(nameof(StringCovariantReturnService), service.Method());
        Assert.Equal(nameof(StringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedMethod());
        Assert.Equal(nameof(StringCovariantReturnService), service.Property);
        Assert.Equal(nameof(StringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedProperty);
    }

    [Fact]
    public void CreateInterfaceProxy_ForBaseInterfaceAndDerivedImplementation_ShouldUseDerivedInterceptedMembers()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<ICovariantReturnService, DerivedStringCovariantReturnService>();
        Assert.Equal(nameof(StringCovariantReturnService), service.Method());
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedMethod());
        Assert.Equal(nameof(StringCovariantReturnService), service.Property);
        Assert.Equal(nameof(DerivedStringCovariantReturnService) + nameof(ReturnTypeInterceptor), service.InterceptedProperty);
    }
}
