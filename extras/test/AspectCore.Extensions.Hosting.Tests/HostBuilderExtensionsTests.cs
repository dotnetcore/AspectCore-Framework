using System;
using Microsoft.Extensions.Hosting;
using Xunit;
using AspectCore.Extensions.Hosting;
using AspectCore.Injector;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Extensions.DependencyInjection;

namespace AspectCore.Extensions.Hosting.Tests
{
    public class HostBuilderExtensionsTests
    {
        [Fact]
        public void UserAspectCore_Container()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseAspectCore();
            var host = hostBuilder.Build();
            Assert.IsAssignableFrom<IServiceResolver>((host.Services as IServiceResolver));
        }

        [Fact]
        public void ConfigureAspectCore()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.ConfigureAspectCore(services =>
            {
                services.AddTransient<IService, Service>();
                services.AddDynamicProxy();
            });
            var host = hostBuilder.Build();
            Assert.IsAssignableFrom<IServiceResolver>((host.Services as IServiceResolver));
        }
    }
}
