using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class ServiceCollectionBuildExtensionsAdditionalTests
    {
        public interface IBuildTestService
        {
            string Get();
        }

        public class BuildTestServiceImpl : IBuildTestService
        {
            public string Get() => "build";
        }

        public class BuildTestInterceptor : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                context.ReturnValue = "intercepted-build";
            }
        }

        // ---- BuildDynamicProxyProvider (no args) ----

        [Fact]
        public void BuildDynamicProxyProvider_ReturnsServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestServiceImpl>();
            var provider = services.BuildDynamicProxyProvider();
            Assert.NotNull(provider);
        }

        [Fact]
        public void BuildDynamicProxyProvider_CanResolveServices()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestServiceImpl>();
            var provider = services.BuildDynamicProxyProvider();
            var svc = provider.GetRequiredService<IBuildTestService>();
            Assert.Equal("build", svc.Get());
        }

        // ---- BuildDynamicProxyProvider (bool validateScopes) ----

        [Fact]
        public void BuildDynamicProxyProvider_WithValidateScopes_ReturnsProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestServiceImpl>();
            var provider = services.BuildDynamicProxyProvider(true);
            Assert.NotNull(provider);
        }

        // ---- BuildDynamicProxyProvider (ServiceProviderOptions) ----

        [Fact]
        public void BuildDynamicProxyProvider_WithOptions_ReturnsProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestServiceImpl>();
            var options = new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true };
            var provider = services.BuildDynamicProxyProvider(options);
            Assert.NotNull(provider);
        }

        // ---- WeaveDynamicProxyService ----

        [Fact]
        public void WeaveDynamicProxyService_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.WeaveDynamicProxyService());
        }

        [Fact]
        public void WeaveDynamicProxyService_ReturnsNewServiceCollection()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestServiceImpl>();
            var result = services.WeaveDynamicProxyService();
            Assert.NotNull(result);
            Assert.NotSame(services, result);
        }

        [Fact]
        public void WeaveDynamicProxyService_PreservesAllServices()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestServiceImpl>();
            services.AddSingleton<string>("hello");
            var result = services.WeaveDynamicProxyService();
            // Original services are preserved (framework adds more services)
            Assert.Contains(result, d => d.ServiceType == typeof(IBuildTestService));
            Assert.Contains(result, d => d.ServiceType == typeof(string));
        }

        [Fact]
        public void WeaveDynamicProxyService_WithInterceptor_ProxiesInterface()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestInterceptorService>();
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(IBuildTestService));
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.ImplementationFactory);
        }

        [Fact]
        public void WeaveDynamicProxyService_WithoutInterceptor_KeepsOriginal()
        {
            var services = new ServiceCollection();
            services.AddTransient<IBuildTestService, BuildTestServiceImpl>();
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(IBuildTestService));
            Assert.NotNull(descriptor);
            Assert.Equal(typeof(BuildTestServiceImpl), descriptor.ImplementationType);
        }

        public class BuildTestInterceptorService : IBuildTestService
        {
            [BuildTestInterceptor]
            public string Get() => "intercepted-original";
        }

        // ---- Scoped service with proxy ----

        public interface IScopedBuildService
        {
            string Get();
        }

        public class ScopedBuildServiceImpl : IScopedBuildService
        {
            [BuildTestInterceptor]
            public string Get() => "scoped-original";
        }

        [Fact]
        public void BuildDynamicProxyProvider_ScopedService_ResolvesInScope()
        {
            var services = new ServiceCollection();
            services.AddScoped<IScopedBuildService, ScopedBuildServiceImpl>();
            var provider = services.BuildDynamicProxyProvider();
            using var scope = provider.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IScopedBuildService>();
            Assert.Equal("intercepted-build", svc.Get());
        }

        // ---- Singleton service with proxy ----

        public interface ISingletonBuildService
        {
            string Get();
        }

        public class SingletonBuildServiceImpl : ISingletonBuildService
        {
            [BuildTestInterceptor]
            public string Get() => "singleton-original";
        }

        [Fact]
        public void BuildDynamicProxyProvider_SingletonService_Resolves()
        {
            var services = new ServiceCollection();
            services.AddSingleton<ISingletonBuildService, SingletonBuildServiceImpl>();
            var provider = services.BuildDynamicProxyProvider();
            var svc = provider.GetRequiredService<ISingletonBuildService>();
            Assert.Equal("intercepted-build", svc.Get());
        }
    }
}
