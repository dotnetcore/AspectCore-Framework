using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Xunit;

namespace AspectCoreTest.Autofac
{
    public class ContainerBuilderExtensionsAdditionalTests
    {
        [Fact]
        public void RegisterDynamicProxy_NullContainerBuilder_Throws()
        {
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.RegisterDynamicProxy());
        }

        [Fact]
        public void RegisterDynamicProxy_WithConfiguration_RegistersConfiguration()
        {
            var builder = new ContainerBuilder();
            var config = new AspectCore.Configuration.AspectConfiguration();
            builder.RegisterDynamicProxy(config);
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            var resolvedConfig = container.Resolve<IAspectConfiguration>();
            Assert.NotNull(resolvedConfig);
        }

        [Fact]
        public void RegisterDynamicProxy_WithConfigure_InvokesConfigure()
        {
            var builder = new ContainerBuilder();
            bool configured = false;
            builder.RegisterDynamicProxy(config => { configured = true; });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            Assert.True(configured);
        }

        [Fact]
        public void RegisterDynamicProxy_WithConfigurationAndConfigure_RegistersBoth()
        {
            var builder = new ContainerBuilder();
            var config = new AspectCore.Configuration.AspectConfiguration();
            bool configured = false;
            builder.RegisterDynamicProxy(config, c => { configured = true; });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            Assert.True(configured);
            Assert.NotNull(container.Resolve<IAspectConfiguration>());
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersInterceptorCollector()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IInterceptorCollector>());
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersProxyGenerator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IProxyGenerator>());
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersAspectContextFactory()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IAspectContextFactory>());
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersManyEnumerable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IManyEnumerable<IService>>());
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersServiceProvider()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            Assert.NotNull(container.Resolve<IServiceProvider>());
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_NullBuilder_Throws()
        {
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureDynamicProxyEngine(options => { }));
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_WithStrict_SetsStrict()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.ConfigureDynamicProxyEngine(options =>
            {
                options.Strict = true;
                options.Engine = ProxyEngine.SourceGenerator;
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            var options = container.Resolve<ProxyEngineOptions>();
            Assert.True(options.Strict);
            Assert.Equal(ProxyEngine.SourceGenerator, options.Engine);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_WithAutoEngine_SetsAuto()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.Auto;
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();

            var options = container.Resolve<ProxyEngineOptions>();
            Assert.Equal(ProxyEngine.Auto, options.Engine);
        }
    }
}
