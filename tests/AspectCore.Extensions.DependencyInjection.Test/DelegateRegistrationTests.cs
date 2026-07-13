using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test;

public class DelegateRegistrationTests
{
    public class TestIntercept : AbstractInterceptorAttribute
    {
        public override Task Invoke(AspectContext context, AspectDelegate next)
        {
            context.ReturnValue = "intercepted";
            return Task.CompletedTask;
        }
    }

    public interface ITestService
    {
        [TestIntercept]
        string GetValue();
    }

    public class TestService : ITestService
    {
        public string GetValue() => "original";
    }

    /// <summary>
    /// Regression test for #235: interceptor should work when the service is
    /// registered via a factory delegate (AddScoped(sp => new Service())).
    /// </summary>
    [Fact]
    public void Interceptor_Works_With_Factory_Registration()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITestService>(sp => new TestService());
        services.ConfigureDynamicProxy(config =>
        {
            config.Interceptors.AddTyped<TestIntercept>();
        });

        var provider = services.BuildDynamicProxyProvider();
        var service = provider.GetRequiredService<ITestService>();

        // The interceptor sets ReturnValue to "intercepted", replacing the original "original".
        Assert.Equal("intercepted", service.GetValue());
    }

    [Fact]
    public void Interceptor_Works_With_Type_Registration()
    {
        var services = new ServiceCollection();
        services.AddScoped<ITestService, TestService>();
        services.ConfigureDynamicProxy(config =>
        {
            config.Interceptors.AddTyped<TestIntercept>();
        });

        var provider = services.BuildDynamicProxyProvider();
        var service = provider.GetRequiredService<ITestService>();

        Assert.Equal("intercepted", service.GetValue());
    }
}
