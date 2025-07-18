using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public class CovariantReturnTypesTests : DynamicProxyTestBase
{
    public interface IService
    {
        object Method();
        object Property { get; }
    }

    public class Service : IService
    {
        public virtual object Method() => nameof(Service);
        public virtual object Property { get; } = nameof(Service);
    }

    public class CovariantReturnsService : Service
    {
        public override string Method() => nameof(CovariantReturnsService);
        public override string Property { get; } = nameof(CovariantReturnsService);
    }

    [Fact]
    public void CreateClassProxy_CovariantReturns_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }

    [Fact]
    public void CreateClassProxy_WithServiceType_CovariantReturns_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<Service, CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }

    [Fact]
    public void CreateInterfaceProxy_CovariantReturns_Test()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IService, CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }
}
