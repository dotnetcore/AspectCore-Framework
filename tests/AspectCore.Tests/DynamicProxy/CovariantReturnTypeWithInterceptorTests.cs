using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public class CovariantReturnTypeWithInterceptorTests : DynamicProxyTestBase
{
    public class BaseService
    {
        public virtual BaseResult Property { get; } = new("base");
        public virtual BaseResult Method() => new("base");
    }

    public class DerivedBaseService : BaseService
    {
        public override BaseResult Property { get; } = new("derived");
        public override BaseResult Method() => new("derived");
    }

    public class CovariantReturnService : BaseService
    {
        public override LeafResult Property { get; } = new("crt");
        public override LeafResult Method() => new("crt");
    }

    protected override void Configure(IAspectConfiguration configuration)
    {
        configuration.Interceptors.AddDelegate(async (ctx, next) =>
        {
            await next(ctx);
            ctx.ReturnValue = new LeafResult(((BaseResult)ctx.ReturnValue).Name + ":intercepted");
        }, Predicates.ForService(nameof(BaseService)));
    }

    [Fact]
    public void CreateClassProxy_ForOrdinaryOverride_ShouldApplyBaseServiceInterceptor()
    {
        var proxy = ProxyGenerator.CreateClassProxy<BaseService, DerivedBaseService>();
        Assert.Equal("derived:intercepted", proxy.Method().Name);
        Assert.Equal("derived:intercepted", proxy.Property.Name);
    }

    [Fact]
    public void CreateClassProxy_ForCovariantOverride_ShouldApplyBaseServiceInterceptor()
    {
        var proxy = ProxyGenerator.CreateClassProxy<BaseService, CovariantReturnService>();
        Assert.Equal("crt:intercepted", proxy.Method().Name);
        Assert.Equal("crt:intercepted", proxy.Property.Name);
    }


    [Fact]
    public void CreateClassProxy_ForCovariantProperty_ShouldKeepPropertyTypeAlignedWithGetterReturnType()
    {
        const BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance;

        var proxy = ProxyGenerator.CreateClassProxy<BaseService, CovariantReturnService>();
        {
            var property = proxy.GetType().GetProperty(nameof(BaseService.Property), flags);
            Assert.NotNull(property);
            Assert.Equal(property.PropertyType, property.GetMethod?.ReturnType);
        }

        var service = new CovariantReturnService();
        {
            var property = service.GetType().GetProperty(nameof(BaseService.Property), flags);
            Assert.NotNull(property);
            Assert.Equal(property.PropertyType, property.GetMethod?.ReturnType);
        }
    }
}
