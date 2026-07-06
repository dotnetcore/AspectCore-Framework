using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using AspectCore.Extensions;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public class MultiLevelCovariantReturnMethodTests : DynamicProxyTestBase
{
    public class ReturnTypeInterceptor : AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectContext context, AspectDelegate next)
        {
            await context.Invoke(next);

            context.ReturnValue = new LeafResult(context.ImplementationMethod.ReturnType.Name);
        }
    }

    public class BaseResult
    {
        public BaseResult(string implementationReturnType)
        {
            ImplementationReturnType = implementationReturnType;
        }

        public string ImplementationReturnType { get; }
    }

    public class MidResult : BaseResult
    {
        public MidResult(string implementationReturnType) : base(implementationReturnType)
        {
        }
    }

    public class LeafResult : MidResult
    {
        public LeafResult(string implementationReturnType) : base(implementationReturnType)
        {
        }
    }

    public class BaseService
    {
        [ReturnTypeInterceptor]
        public virtual BaseResult Create() => new(nameof(BaseResult));
    }

    public class MidService : BaseService
    {
        public override MidResult Create() => new(nameof(MidResult));
    }

    public class LeafService : MidService
    {
        public override LeafResult Create() => new(nameof(LeafResult));
    }

    [Fact]
    public void CreateClassProxy_MultiLevelCovariantReturn_UsesLeafOverride_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<BaseService, LeafService>();

        var result = service.Create();

        Assert.Equal(nameof(LeafResult), result.ImplementationReturnType);
    }

    [Fact]
    public void GetCovariantReturnMethods_MultiLevelCovariantReturn_ReportsOnlyLeafOverride_Test()
    {
        var baseMethod = typeof(BaseService).GetMethod(nameof(BaseService.Create))!;
        var methods = typeof(LeafService).GetCovariantReturnMethods()
            .Where(x => x.OverriddenMethod.GetBaseDefinition() == baseMethod)
            .ToArray();

        var method = Assert.Single(methods);
        Assert.Equal(typeof(LeafResult), method.CovariantReturnMethod.ReturnType);
    }
}
