using AspectCore.DynamicProxy;
using AspectCore.Tests.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.Issues.DynamicProxy;

// https://github.com/dotnetcore/AspectCore-Framework/issues/223
public class InterfaceDefaultMethodsShouldBeUsedTests : DynamicProxyTestBase
{
    public interface IService
    {
        int Get() => 1;
    }

    public class Service : IService{}

    [Fact]
    public void CreateInterfaceProxy_WithoutImplementationType_Test()
    {
        var service = ProxyGenerator.CreateInterfaceProxy<IService>();
        var result = service.Get();
        Assert.Equal(1, result);
    }
}