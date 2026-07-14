using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using AspectCore.DependencyInjection;
using Extensions.Hosting.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AspectCore.Extensions.Hosting.Tests
{
    public class HostBuilderExtensionsAdditionalTests
    {
        [Fact]
        public void UseServiceContext_WithAction_NullHostBuilder_ThrowsArgumentNullException()
        {
            IHostBuilder hostBuilder = null;
            Assert.Throws<ArgumentNullException>(() => hostBuilder.UseServiceContext(ctx => { }));
        }

        [Fact]
        public void UseServiceContext_WithHostBuilderContextAction_NullHostBuilder_ThrowsArgumentNullException()
        {
            IHostBuilder hostBuilder = null;
            Assert.Throws<ArgumentNullException>(() => hostBuilder.UseServiceContext((ctx, services) => { }));
        }

        [Fact]
        public void UseServiceContext_WithHostBuilderContextAction_RegistersServices()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseServiceContext((hostContext, context) =>
            {
                context.AddType<IService, Service>();
            });
            var host = hostBuilder.Build();
            var service = host.Services.GetService<IService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
            host.Dispose();
        }

        [Fact]
        public void UseServiceContext_WithHostBuilderContextAction_ConfiguresInterceptor()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseServiceContext((hostContext, context) =>
            {
                context.AddType<IService, Service>();
                context.Configure(config =>
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
            host.Dispose();
        }

        [Fact]
        public void UseServiceContext_WithNullConfigureDelegate_DoesNotThrow()
        {
            var hostBuilder = new HostBuilder();
            Action<IServiceContext> configureDelegate = null;
            hostBuilder.UseServiceContext(configureDelegate);
            var host = hostBuilder.Build();
            Assert.NotNull(host);
            host.Dispose();
        }

        [Fact]
        public void UseServiceContext_WithHostBuilderContextNullConfigureDelegate_DoesNotThrow()
        {
            var hostBuilder = new HostBuilder();
            Action<HostBuilderContext, IServiceContext> configureDelegate = null;
            hostBuilder.UseServiceContext(configureDelegate);
            var host = hostBuilder.Build();
            Assert.NotNull(host);
            host.Dispose();
        }

        [Fact]
        public void UseDynamicProxy_NullHostBuilder_ThrowsArgumentNullException()
        {
            IHostBuilder hostBuilder = null;
            Assert.Throws<ArgumentNullException>(() => hostBuilder.UseDynamicProxy());
        }

        [Fact]
        public void UseDynamicProxy_BuildsHostSuccessfully()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy();
            var host = hostBuilder.Build();
            Assert.NotNull(host);
            host.Dispose();
        }

        [Fact]
        public void ConfigureDynamicProxy_WithHostBuilderContext_NullHostBuilder_ThrowsArgumentNullException()
        {
            IHostBuilder hostBuilder = null;
            Assert.Throws<ArgumentNullException>(() => hostBuilder.ConfigureDynamicProxy((ctx, services, config) => { }));
        }

        [Fact]
        public void ConfigureDynamicProxy_WithHostBuilderContext_NullConfigureDelegate_ThrowsArgumentNullException()
        {
            var hostBuilder = new HostBuilder();
            Action<HostBuilderContext, IServiceCollection, IAspectConfiguration> configureDelegate = null;
            Assert.Throws<ArgumentNullException>(() => hostBuilder.ConfigureDynamicProxy(configureDelegate));
        }

        [Fact]
        public void ConfigureDynamicProxy_WithHostBuilderContext_RegistersServicesAndConfigures()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy().ConfigureDynamicProxy((hostContext, services, config) =>
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
            Assert.NotNull(service);
            Assert.True(service.IsProxy());
            host.Dispose();
        }

        [Fact]
        public void ConfigureDynamicProxy_NullHostBuilder_ThrowsArgumentNullException()
        {
            IHostBuilder hostBuilder = null;
            Assert.Throws<ArgumentNullException>(() => hostBuilder.ConfigureDynamicProxy((services, config) => { }));
        }

        [Fact]
        public void ConfigureDynamicProxy_NullConfigureDelegate_DoesNotThrow()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy();
            Action<IServiceCollection, IAspectConfiguration> configureDelegate = null;
            hostBuilder.ConfigureDynamicProxy(configureDelegate);
            var host = hostBuilder.Build();
            Assert.NotNull(host);
            host.Dispose();
        }

        [Fact]
        public void ConfigureDynamicProxy_RegistersServicesAndConfigures()
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
            Assert.NotNull(service);
            Assert.True(service.IsProxy());
            host.Dispose();
        }

        [Fact]
        public void UseServiceContext_WithAction_RegistersServices()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseServiceContext(context =>
            {
                context.AddType<IService, Service>();
            });
            var host = hostBuilder.Build();
            var service = host.Services.GetService<IService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
            host.Dispose();
        }
    }
}
