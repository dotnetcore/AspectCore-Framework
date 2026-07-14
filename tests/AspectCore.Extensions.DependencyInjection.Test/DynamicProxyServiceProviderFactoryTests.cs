using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class DynamicProxyServiceProviderFactoryTests
    {
        public interface IFactoryTestService
        {
            string GetName();
        }

        public class FactoryTestService : IFactoryTestService
        {
            public string GetName() => "factory";
        }

        [Fact]
        public void DefaultConstructor_CreatesInstance()
        {
            var factory = new DynamicProxyServiceProviderFactory();
            Assert.NotNull(factory);
        }

        [Fact]
        public void ValidateScopesConstructor_CreatesInstance()
        {
            var factory = new DynamicProxyServiceProviderFactory(true);
            Assert.NotNull(factory);
        }

        [Fact]
        public void ServiceProviderOptionsConstructor_CreatesInstance()
        {
            var options = new ServiceProviderOptions { ValidateScopes = true };
            var factory = new DynamicProxyServiceProviderFactory(options);
            Assert.NotNull(factory);
        }

        [Fact]
        public void CreateBuilder_ReturnsSameServiceCollection()
        {
            var factory = new DynamicProxyServiceProviderFactory();
            var services = new ServiceCollection();
            var result = factory.CreateBuilder(services);
            Assert.Same(services, result);
        }

        [Fact]
        public void CreateServiceProvider_WithNullOptions_BuildsProvider()
        {
            var factory = new DynamicProxyServiceProviderFactory();
            var services = new ServiceCollection();
            services.AddTransient<IFactoryTestService, FactoryTestService>();
            var builder = factory.CreateBuilder(services);
            var provider = factory.CreateServiceProvider(builder);
            Assert.NotNull(provider);
            var svc = provider.GetRequiredService<IFactoryTestService>();
            Assert.Equal("factory", svc.GetName());
        }

        [Fact]
        public void CreateServiceProvider_WithValidateScopes_BuildsProvider()
        {
            var factory = new DynamicProxyServiceProviderFactory(true);
            var services = new ServiceCollection();
            services.AddTransient<IFactoryTestService, FactoryTestService>();
            var builder = factory.CreateBuilder(services);
            var provider = factory.CreateServiceProvider(builder);
            Assert.NotNull(provider);
            var svc = provider.GetRequiredService<IFactoryTestService>();
            Assert.Equal("factory", svc.GetName());
        }

        [Fact]
        public void CreateServiceProvider_WithServiceProviderOptions_BuildsProvider()
        {
            var options = new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true };
            var factory = new DynamicProxyServiceProviderFactory(options);
            var services = new ServiceCollection();
            services.AddTransient<IFactoryTestService, FactoryTestService>();
            var builder = factory.CreateBuilder(services);
            var provider = factory.CreateServiceProvider(builder);
            Assert.NotNull(provider);
            var svc = provider.GetRequiredService<IFactoryTestService>();
            Assert.Equal("factory", svc.GetName());
        }
    }
}
