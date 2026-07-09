using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public class CovariantReturnTypeWithInterceptorTests : DynamicProxyTestBase
{
    public class BaseService
    {
        public virtual BaseResult Method() => new("base");
    }

    public class DerivedBaseService : BaseService
    {
        public override BaseResult Method() => new("derived");
    }

    public class LeafService : BaseService
    {
        public override LeafResult Method() => new("leaf");
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
    public void Test1()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseService, DerivedBaseService>();
        Assert.Equal("derived:intercepted", service.Method().Name);
    }

    [Fact]
    public void Test2()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseService, LeafService>();
        Assert.Equal("leaf:intercepted", service.Method().Name);
    }
}
