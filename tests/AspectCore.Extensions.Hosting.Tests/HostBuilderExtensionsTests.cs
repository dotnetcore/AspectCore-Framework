using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Extensions.Hosting.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AspectCore.Extensions.Hosting.Tests
{
    public class HostBuilderExtensionsTests
    {
        [Fact]
        public void UserAspectCore_Container()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseServiceContext();
            var host = hostBuilder.Build();
            Assert.IsAssignableFrom<IServiceResolver>((host.Services as IServiceResolver));
            host.Dispose();
        }

        [Fact]
        public void UseAspectCore_Configure()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseServiceContext(container =>
            {
                container.AddType<IService, Service>();
                container.Configure(config =>
                {
                    config.Interceptors.AddDelegate(async (ctx, next) =>
                    {
                        await next(ctx);
                        ctx.ReturnValue = "proxy";
                    });
                });
            });
            var host = hostBuilder.Build();
            var service = host.Services.GetService<IService>();
            Assert.True(service.IsProxy());
        }

        [Fact]
        public void ConfigureDynamicProxy()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy().ConfigureDynamicProxy((services, config) =>
            {
                services.AddTransient<IService, Service>();
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await next(ctx);
                    ctx.ReturnValue = "proxy";
                });
            });
            var host = hostBuilder.Build();
            var service = host.Services.GetService<IService>();
            Assert.True(service.IsProxy());
        }
    }
}
