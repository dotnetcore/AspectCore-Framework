using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Extensions.Hosting.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace AspectCore.Extensions.Hosting.Tests
{
    /// <summary>
    /// E2E tests for AspectCore + Hosting integration: full host builder pipeline,
    /// UseServiceContext, ConfigureDynamicProxy, proxied service resolution through
    /// the host, and hosted service interceptor behavior. Real host builder,
    /// real DI container, real proxies — no mocks.
    /// </summary>
    public class E2EScenarios
    {
        [Fact]
        public void FullHostBuilder_UseServiceContext_ResolveProxiedService_Works()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseServiceContext(context =>
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

            using var host = hostBuilder.Build();
            var service = host.Services.GetService<IService>();

            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
            Assert.True(service.IsProxy());
            Assert.Equal("proxy", service.GetValue());
        }

        [Fact]
        public void ConfigureDynamicProxy_OnHostBuilder_ProxiesService()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy().ConfigureDynamicProxy((services, config) =>
            {
                services.AddTransient<IService, Service>();
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await next(ctx);
                    ctx.ReturnValue = "proxied-value";
                });
            });

            using var host = hostBuilder.Build();
            var service = host.Services.GetService<IService>();

            Assert.NotNull(service);
            Assert.True(service.IsProxy());
            Assert.Equal("proxied-value", service.GetValue());
        }

        [Fact]
        public void HostBuilder_InterceptorExecution_ThroughHostService_Works()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy().ConfigureDynamicProxy((services, config) =>
            {
                services.AddTransient<IService, Service>();
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Before");
                    await next(ctx);
                    E2ELog.Entries.Add("After");
                });
            });

            using var host = hostBuilder.Build();
            E2ELog.Clear();
            var service = host.Services.GetService<IService>();
            var result = service.GetValue();

            Assert.Equal("service", result);
            Assert.Contains("Before", E2ELog.Entries);
            Assert.Contains("After", E2ELog.Entries);
        }

        [Fact]
        public void HostBuilder_ScopedService_ThroughHost_Works()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy().ConfigureDynamicProxy((services, config) =>
            {
                services.AddScoped<IService, Service>();
                config.Interceptors.AddDelegate((ctx, next) => next(ctx));
            });

            using var host = hostBuilder.Build();

            using var scope1 = host.Services.CreateScope();
            var s1 = scope1.ServiceProvider.GetService<IService>();
            var s2 = scope1.ServiceProvider.GetService<IService>();

            // Same scope → same instance.
            Assert.Same(s1, s2);
            Assert.NotNull(s1);

            using var scope2 = host.Services.CreateScope();
            var s3 = scope2.ServiceProvider.GetService<IService>();

            // Different scope → different instance.
            Assert.NotSame(s1, s3);
        }

        [Fact]
        public void HostBuilder_MultipleInterceptors_AllExecute()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseDynamicProxy().ConfigureDynamicProxy((services, config) =>
            {
                services.AddTransient<IService, Service>();
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("First.Before");
                    await next(ctx);
                    E2ELog.Entries.Add("First.After");
                });
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Second.Before");
                    await next(ctx);
                    E2ELog.Entries.Add("Second.After");
                });
            });

            using var host = hostBuilder.Build();
            E2ELog.Clear();
            var service = host.Services.GetService<IService>();
            service.GetValue();

            Assert.Equal("First.Before", E2ELog.Entries[0]);
            Assert.Equal("Second.Before", E2ELog.Entries[1]);
            Assert.Equal("Second.After", E2ELog.Entries[2]);
            Assert.Equal("First.After", E2ELog.Entries[3]);
        }

        [Fact]
        public void HostBuilder_UseServiceContext_ResolvesIServiceResolver()
        {
            var hostBuilder = new HostBuilder();
            hostBuilder.UseServiceContext(context =>
            {
                context.AddType<IService, Service>();
            });

            using var host = hostBuilder.Build();

            // The host services should be an IServiceResolver.
            var resolver = host.Services as IServiceResolver;
            Assert.NotNull(resolver);

            var service = resolver!.Resolve<IService>();
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        /// <summary>
        /// Shared log for E2E interceptor execution verification.
        /// </summary>
        public static class E2ELog
        {
            public static readonly List<string> Entries = new();
            public static void Clear() => Entries.Clear();
        }
    }
}
