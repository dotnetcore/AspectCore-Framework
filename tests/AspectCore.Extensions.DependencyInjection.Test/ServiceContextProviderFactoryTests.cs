using System;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class ServiceContextProviderFactoryTests
    {
        public interface IContextTestService
        {
            string GetValue();
        }

        public class ContextTestService : IContextTestService
        {
            public string GetValue() => "context";
        }

        [Fact]
        public void CreateBuilder_ReturnsServiceContext()
        {
            var factory = new ServiceContextProviderFactory();
            var services = new ServiceCollection();
            services.AddTransient<IContextTestService, ContextTestService>();
            var context = factory.CreateBuilder(services);
            Assert.NotNull(context);
            Assert.IsAssignableFrom<IServiceContext>(context);
        }

        [Fact]
        public void CreateServiceProvider_ReturnsServiceProvider()
        {
            var factory = new ServiceContextProviderFactory();
            var services = new ServiceCollection();
            services.AddTransient<IContextTestService, ContextTestService>();
            var context = factory.CreateBuilder(services);
            var provider = factory.CreateServiceProvider(context);
            Assert.NotNull(provider);
        }

        [Fact]
        public void CreateServiceProvider_ResolvesRegisteredService()
        {
            var factory = new ServiceContextProviderFactory();
            var services = new ServiceCollection();
            services.AddTransient<IContextTestService, ContextTestService>();
            var context = factory.CreateBuilder(services);
            var provider = factory.CreateServiceProvider(context);
            var svc = provider.GetRequiredService<IContextTestService>();
            Assert.Equal("context", svc.GetValue());
        }

        [Fact]
        public void CreateBuilder_WithEmptyServices_Works()
        {
            var factory = new ServiceContextProviderFactory();
            var services = new ServiceCollection();
            var context = factory.CreateBuilder(services);
            Assert.NotNull(context);
            var provider = factory.CreateServiceProvider(context);
            Assert.NotNull(provider);
        }
    }
}
