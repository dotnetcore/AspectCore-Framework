using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.LightInject;
using AspectCoreTest.LightInject.Fakes;
using LightInject;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class ContainerBuilderExtensionsAdditionalTests
    {
        [Fact]
        public void RegisterDynamicProxy_NullContainer_Throws()
        {
            IServiceContainer container = null;
            Assert.Throws<ArgumentNullException>(() => container.RegisterDynamicProxy());
        }

        [Fact]
        public void RegisterDynamicProxy_WithConfiguration_NullContainer_Throws()
        {
            IServiceContainer container = null;
            Assert.Throws<ArgumentNullException>(() => container.RegisterDynamicProxy(null, config => { }));
        }

        [Fact]
        public void RegisterDynamicProxy_WithConfiguration_UsesProvidedConfiguration()
        {
            var container = new ServiceContainer();
            var config = new AspectConfiguration();
            config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.LightInject.Fakes"));
            container.RegisterDynamicProxy(config);
            container.Register<IService, Service>();
            var service = container.GetInstance<IService>();
            Assert.NotNull(service);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_NullContainer_Throws()
        {
            IServiceContainer container = null;
            Assert.Throws<ArgumentNullException>(() => container.ConfigureDynamicProxyEngine(options => { }));
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_RegistersOptionsAndGenerator()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy();
            container.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.SourceGenerator;
                options.Strict = true;
            });
            container.Register<IService, Service>();
            var options = container.GetInstance<ProxyEngineOptions>();
            Assert.Equal(ProxyEngine.SourceGenerator, options.Engine);
            Assert.True(options.Strict);
            var generator = container.GetInstance<IProxyTypeGenerator>();
            Assert.NotNull(generator);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_NullConfigure_IsAllowed()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy();
            container.ConfigureDynamicProxyEngine(null);
            container.Register<IService, Service>();
            var options = container.GetInstance<ProxyEngineOptions>();
            Assert.Equal(ProxyEngine.DynamicProxy, options.Engine);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_WithAutoEngine_RegistersGenerator()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy();
            container.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.Auto;
                options.AllowRuntimeFallback = true;
                options.Strict = false;
            });
            container.Register<IService, Service>();
            var service = container.GetInstance<IService>();
            Assert.NotNull(service);
        }
    }

    public class LightInjectServiceResolverAdditionalTests
    {
        private static IServiceResolver CreateResolver()
        {
            var container = new ServiceContainer().RegisterDynamicProxy();
            container.Register<IService, Service>();
            return container.GetInstance<IServiceResolver>();
        }

        [Fact]
        public void GetService_ReturnsRegisteredService()
        {
            var resolver = CreateResolver();
            var serviceProvider = (IServiceProvider)resolver;
            var service = serviceProvider.GetService(typeof(IService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void GetService_ReturnsNullForUnregisteredService()
        {
            var resolver = CreateResolver();
            var serviceProvider = (IServiceProvider)resolver;
            var service = serviceProvider.GetService(typeof(IUnregisteredService));
            Assert.Null(service);
        }

#if NET8_0_OR_GREATER
        private static IServiceResolver CreateKeyedResolver()
        {
            var container = new ServiceContainer().RegisterDynamicProxy();
            container.Register<IService, Service>();
            container.Register<IService, Service>("key");
            return container.GetInstance<IServiceResolver>();
        }

        [Fact]
        public void GetKeyedService_ReturnsKeyedService()
        {
            var resolver = CreateKeyedResolver();
            var keyed = (IKeyedServiceProvider)resolver;
            var svc = keyed.GetKeyedService(typeof(IService), "key");
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IService>(svc);
        }

        [Fact]
        public void GetKeyedService_ReturnsNull_ForUnregisteredKey()
        {
            var resolver = CreateKeyedResolver();
            var keyed = (IKeyedServiceProvider)resolver;
            var result = keyed.GetKeyedService(typeof(IService), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void GetRequiredKeyedService_ReturnsKeyedService()
        {
            var resolver = CreateKeyedResolver();
            var keyed = (IKeyedServiceProvider)resolver;
            var svc = keyed.GetRequiredKeyedService(typeof(IService), "key");
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IService>(svc);
        }

        [Fact]
        public void GetRequiredKeyedService_Throws_ForUnregisteredKey()
        {
            var resolver = CreateKeyedResolver();
            var keyed = (IKeyedServiceProvider)resolver;
            Assert.Throws<InvalidOperationException>(() => keyed.GetRequiredKeyedService(typeof(IService), "missing"));
        }
#endif
    }

    public class LightInjectGenericServiceTests
    {
        [Fact]
        public void RegisterDynamicProxy_GenericTypeDefinition_CreatesProxy()
        {
            var container = new ServiceContainer();
            container.RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCoreTest.LightInject"));
            });
            container.Register(typeof(IGenericService<>), typeof(GenericService<>));
            var service = container.GetInstance<IGenericService<int>>();
            Assert.NotNull(service);
        }
    }

    public interface IGenericService<T>
    {
        T GetValue(T input);
    }

    public class GenericService<T> : IGenericService<T>
    {
        public virtual T GetValue(T input)
        {
            return input;
        }
    }

    public interface IUnregisteredService
    {
    }
}
