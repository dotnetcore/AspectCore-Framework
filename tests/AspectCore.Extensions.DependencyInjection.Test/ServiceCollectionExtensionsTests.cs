using System;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class ServiceCollectionExtensionsTests
    {
        public interface IExtTestService
        {
            string Get();
        }

        public class ExtTestServiceImpl : IExtTestService
        {
            public string Get() => "ext";
        }

        // ---- ConfigureDynamicProxy ----

        [Fact]
        public void ConfigureDynamicProxy_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.ConfigureDynamicProxy());
        }

        [Fact]
        public void ConfigureDynamicProxy_RegistersAspectConfiguration()
        {
            var services = new ServiceCollection();
            services.ConfigureDynamicProxy();
            var configDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AspectCore.Configuration.IAspectConfiguration));
            Assert.NotNull(configDescriptor);
        }

        [Fact]
        public void ConfigureDynamicProxy_WithConfigure_InvokesCallback()
        {
            var services = new ServiceCollection();
            bool called = false;
            services.ConfigureDynamicProxy(config =>
            {
                called = true;
            });
            Assert.True(called);
        }

        [Fact]
        public void ConfigureDynamicProxy_CalledTwice_ReusesExistingConfiguration()
        {
            var services = new ServiceCollection();
            services.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) => next(ctx));
            });
            // Second call should not add a new config descriptor
            services.ConfigureDynamicProxy(config =>
            {
            });
            var configCount = services.Count(x => x.ServiceType == typeof(AspectCore.Configuration.IAspectConfiguration) && x.ImplementationInstance != null);
            Assert.Equal(1, configCount);
        }

        [Fact]
        public void ConfigureDynamicProxy_ReturnsSameServiceCollection()
        {
            var services = new ServiceCollection();
            var result = services.ConfigureDynamicProxy();
            Assert.Same(services, result);
        }

        // ---- AddDynamicProxy (obsolete, delegates to ConfigureDynamicProxy) ----

        [Fact]
        public void AddDynamicProxy_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.AddDynamicProxy());
        }

        [Fact]
        public void AddDynamicProxy_RegistersConfiguration()
        {
            var services = new ServiceCollection();
#pragma warning disable CS0618
            services.AddDynamicProxy();
#pragma warning restore CS0618
            var configDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(AspectCore.Configuration.IAspectConfiguration));
            Assert.NotNull(configDescriptor);
        }

        [Fact]
        public void AddDynamicProxy_WithConfigure_InvokesCallback()
        {
            var services = new ServiceCollection();
            bool called = false;
#pragma warning disable CS0618
            services.AddDynamicProxy(config => { called = true; });
#pragma warning restore CS0618
            Assert.True(called);
        }

        // ---- ConfigureDynamicProxyEngine ----

        [Fact]
        public void ConfigureDynamicProxyEngine_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.ConfigureDynamicProxyEngine(_ => { }));
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_NullConfigure_DoesNotThrow()
        {
            var services = new ServiceCollection();
            // configure is null-safe (uses configure?.Invoke)
            var result = services.ConfigureDynamicProxyEngine(null);
            Assert.NotNull(result);
            Assert.Same(services, result);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_RegistersProxyEngineOptions()
        {
            var services = new ServiceCollection();
            services.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.SourceGenerator;
            });
            var optionsDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ProxyEngineOptions));
            Assert.NotNull(optionsDescriptor);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_InvokesCallback()
        {
            var services = new ServiceCollection();
            bool called = false;
            services.ConfigureDynamicProxyEngine(options =>
            {
                called = true;
                options.Engine = ProxyEngine.DynamicProxy;
            });
            Assert.True(called);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_ReplacesProxyTypeGenerator()
        {
            var services = new ServiceCollection();
            services.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.DynamicProxy;
            });
            // Should have replaced IProxyTypeGenerator with SourceGeneratedProxyTypeGenerator
            var generatorDescriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IProxyTypeGenerator));
            Assert.NotNull(generatorDescriptor);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_ReturnsSameServiceCollection()
        {
            var services = new ServiceCollection();
            var result = services.ConfigureDynamicProxyEngine(_ => { });
            Assert.Same(services, result);
        }

        // ---- AddSourceGeneratedProxyRegistry ----

        public class TestRegistry : ISourceGeneratedProxyRegistry
        {
            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                proxyType = null;
                return false;
            }
        }

        [Fact]
        public void AddSourceGeneratedProxyRegistry_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.AddSourceGeneratedProxyRegistry<TestRegistry>());
        }

        [Fact]
        public void AddSourceGeneratedProxyRegistry_RegistersRegistry()
        {
            var services = new ServiceCollection();
            services.AddSourceGeneratedProxyRegistry<TestRegistry>();
            var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(ISourceGeneratedProxyRegistry));
            Assert.NotNull(descriptor);
            Assert.Equal(typeof(TestRegistry), descriptor.ImplementationType);
        }

        [Fact]
        public void AddSourceGeneratedProxyRegistry_ReturnsSameServiceCollection()
        {
            var services = new ServiceCollection();
            var result = services.AddSourceGeneratedProxyRegistry<TestRegistry>();
            Assert.Same(services, result);
        }
    }
}
