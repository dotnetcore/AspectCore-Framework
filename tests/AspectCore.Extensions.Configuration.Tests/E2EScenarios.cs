using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Configuration;
using AspectCore.Extensions.DependencyInjection;
using AspectCoreTest.Configuration.E2E;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.Configuration.Tests
{
    /// <summary>
    /// E2E tests for AspectCore Configuration integration: configuration binding
    /// injection via the built-in container, configuration values injected into
    /// proxied services via MS DI container, and interceptor behavior on
    /// configuration-dependent services. Real configuration, real DI containers,
    /// real proxies — no mocks.
    /// </summary>
    public class E2EScenarios
    {
        private static IConfiguration BuildConfiguration(Dictionary<string, string> dict)
        {
            var builder = new ConfigurationBuilder().AddEnvironmentVariables();
            builder.AddInMemoryCollection(dict);
            return builder.Build();
        }

        [Fact]
        public void ConfigurationBinding_Injection_BuiltInContainer_Works()
        {
            var dict = new Dictionary<string, string>
            {
                {"app:name", "TestApp"},
                {"app:version", "1.0.0"}
            };
            var configuration = BuildConfiguration(dict);

            var container = new ServiceContext();
            container.AddInstance<IConfiguration>(configuration);
            container.AddConfigurationInject();
            container.AddType<IConfiguredAppService, ConfiguredAppService>();

            using var resolver = container.Build();
            var service = resolver.Resolve<IConfiguredAppService>();

            Assert.NotNull(service);
            Assert.Equal("TestApp", service.GetAppName());
            Assert.Equal("1.0.0", service.GetVersion());
        }

        [Fact]
        public void ConfigurationValue_Injection_BuiltInContainer_Works()
        {
            var dict = new Dictionary<string, string>
            {
                {"db:connectionString", "Server=localhost;Database=test"},
                {"db:timeout", "30"}
            };
            var configuration = BuildConfiguration(dict);

            var container = new ServiceContext();
            container.AddInstance<IConfiguration>(configuration);
            container.AddConfigurationInject();
            container.AddType<IDbConfigService, DbConfigService>();

            using var resolver = container.Build();
            var service = resolver.Resolve<IDbConfigService>();

            Assert.NotNull(service);
            Assert.Equal("Server=localhost;Database=test", service.GetConnectionString());
            Assert.Equal(30, service.GetTimeout());
        }

        [Fact]
        public void ConfigurationBinding_AndValue_BothWork_SameService()
        {
            var dict = new Dictionary<string, string>
            {
                {"feature:name", "E2EFeature"},
                {"feature:enabled", "true"},
                {"feature:maxRetries", "5"}
            };
            var configuration = BuildConfiguration(dict);

            var container = new ServiceContext();
            container.AddInstance<IConfiguration>(configuration);
            container.AddConfigurationInject();
            container.AddType<IFeatureService, FeatureService>();

            using var resolver = container.Build();
            var service = resolver.Resolve<IFeatureService>();

            Assert.NotNull(service);
            Assert.Equal("E2EFeature", service.GetName());
            Assert.Equal(5, service.GetMaxRetries());
        }

        [Fact]
        public void ConfigurationInjection_ProxiedService_MsDiContainer_Works()
        {
            var dict = new Dictionary<string, string>
            {
                {"app:name", "ProxiedApp"},
                {"app:version", "2.0.0"},
                {"db:connectionString", "Server=localhost;Database=test"}
            };
            var configuration = BuildConfiguration(dict);

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IProxiedConfigService, ProxiedConfigService>();
            services.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    E2ELog.Entries.Add("Before");
                    await next(ctx);
                    E2ELog.Entries.Add("After");
                });
            });

            using var provider = services.BuildDynamicProxyProvider();
            E2ELog.Clear();
            var service = provider.GetRequiredService<IProxiedConfigService>();

            Assert.NotNull(service);
            Assert.IsNotType<ProxiedConfigService>(service);
            Assert.Equal("ProxiedApp", service.GetAppName());
            Assert.Equal("2.0.0", service.GetVersion());
            Assert.Equal("Server=localhost;Database=test", service.GetConnectionString());
            Assert.Contains("Before", E2ELog.Entries);
            Assert.Contains("After", E2ELog.Entries);
        }

        [Fact]
        public void ConfigurationInjection_ProxiedService_ReturnValueModification_Works()
        {
            var dict = new Dictionary<string, string>
            {
                {"app:name", "ReturnValueApp"},
                {"app:version", "3.0.0"},
                {"db:connectionString", "Server=localhost;Database=test"}
            };
            var configuration = BuildConfiguration(dict);

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IProxiedConfigService, ProxiedConfigService>();
            services.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(async (ctx, next) =>
                {
                    await next(ctx);
                    if (ctx.ReturnValue is string s)
                    {
                        ctx.ReturnValue = s + "-modified";
                    }
                });
            });

            using var provider = services.BuildDynamicProxyProvider();
            var service = provider.GetRequiredService<IProxiedConfigService>();

            Assert.IsNotType<ProxiedConfigService>(service);
            Assert.Equal("ReturnValueApp-modified", service.GetAppName());
            Assert.Equal("3.0.0-modified", service.GetVersion());
        }

        [Fact]
        public void ConfigurationInjection_MultipleProxiedServices_AllGetConfig()
        {
            var dict = new Dictionary<string, string>
            {
                {"app:name", "SharedApp"},
                {"app:version", "1.0.0"},
                {"db:connectionString", "SharedConn"}
            };
            var configuration = BuildConfiguration(dict);

            var services = new ServiceCollection();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddScoped<IProxiedConfigService, ProxiedConfigService>();
            services.AddScoped<IConfiguredAppService, ConfiguredAppService>();
            services.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx));
            });

            using var provider = services.BuildDynamicProxyProvider();
            var proxiedService = provider.GetRequiredService<IProxiedConfigService>();

            Assert.NotNull(proxiedService);
            Assert.IsNotType<ProxiedConfigService>(proxiedService);
            Assert.Equal("SharedApp", proxiedService.GetAppName());
            Assert.Equal("SharedConn", proxiedService.GetConnectionString());
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
