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
    public void Test1()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseService, DerivedBaseService>();
        Assert.Equal("derived:intercepted", service.Method().Name);
        Assert.Equal("derived:intercepted", service.Property.Name);
    }

    [Fact]
    public void Test2()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseService, CovariantReturnService>();
        Assert.Equal("crt:intercepted", service.Method().Name);
        Assert.Equal("crt:intercepted", service.Property.Name);
    }
}
