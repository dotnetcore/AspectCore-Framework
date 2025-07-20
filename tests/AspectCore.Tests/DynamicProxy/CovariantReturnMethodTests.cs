using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy;

public class CovariantReturnMethodTests : DynamicProxyTestBase
{
    public interface IService
    {
        object Method();
        object Property { get; }
    }

    public class Service : IService
    {
        public virtual object Method() => new();
        public virtual object Property { get; } = new();
    }

    public class CovariantReturnsService : Service
    {
        public override string Method() => nameof(CovariantReturnsService);
        public override string Property { get; } = nameof(CovariantReturnsService);
    }

    public class DerivedCovariantReturnsService : CovariantReturnsService
    {
        public override string Method() => nameof(DerivedCovariantReturnsService);
    }

    [Fact]
    public void CreateClassProxy_CovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }

    [Fact]
    public void CreateClassProxy_DerivedCovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<DerivedCovariantReturnsService>();
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }

    [Fact]
    public void CreateClassProxy_Service_CovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<Service, CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }

    [Fact]
    public void CreateClassProxy_Service_DerivedCovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<Service, DerivedCovariantReturnsService>();
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }

    [Fact]
    public void CreateClassProxy_CovariantReturnsService_DerivedCovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateClassProxy<CovariantReturnsService, DerivedCovariantReturnsService>();
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }
    
    [Fact]
    public void CreateInterfaceProxy_IService_CovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IService, CovariantReturnsService>();
        Assert.Equal(nameof(CovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }

    [Fact]
    public void CreateInterfaceProxy_IService_DerivedCovariantReturnsService_Test()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IService, DerivedCovariantReturnsService>();
        Assert.Equal(nameof(DerivedCovariantReturnsService), service.Method());
        Assert.Equal(nameof(CovariantReturnsService), service.Property);
    }
}
