using System;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.Extensions.Autofac;
using AspectCore.Extensions.Test.Fakes;
using Autofac;
using Autofac.Core.Registration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCoreTest.Autofac
{
    public class AutofacResolverTests
    {
        private static IContainer BuildContainer(Action<ContainerBuilder> beforeBuild = null)
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<Service>().As<IService>();
            beforeBuild?.Invoke(builder);
            return builder.Build();
        }

        [Fact]
        public void ScopeResolverFactory_CanBeResolved()
        {
            var container = BuildContainer();
            var factory = container.Resolve<IScopeResolverFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void ScopeResolverFactory_CreateScope_ReturnsServiceResolver()
        {
            var container = BuildContainer();
            var factory = container.Resolve<IScopeResolverFactory>();
            using (var scope = factory.CreateScope())
            {
                Assert.NotNull(scope);
                var service = scope.Resolve(typeof(IService));
                Assert.NotNull(service);
                Assert.IsAssignableFrom<IService>(service);
            }
        }

        [Fact]
        public void ScopeResolverFactory_CreateScope_ScopedServiceIsDifferentAcrossScopes()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.RegisterType<ScopedObject>().InstancePerLifetimeScope();
            var container = builder.Build();
            var factory = container.Resolve<IScopeResolverFactory>();

            IServiceResolver scope1;
            IServiceResolver scope2;
            object fromScope1;
            object fromScope2;
            using (scope1 = factory.CreateScope())
            {
                fromScope1 = scope1.Resolve(typeof(ScopedObject));
            }
            using (scope2 = factory.CreateScope())
            {
                fromScope2 = scope2.Resolve(typeof(ScopedObject));
            }
            Assert.NotSame(fromScope1, fromScope2);
        }

        [Fact]
        public void ServiceResolver_GetService_ReturnsRegisteredService()
        {
            var container = BuildContainer();
            var resolver = container.Resolve<IServiceResolver>();
            var service = resolver.GetService(typeof(IService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void ServiceResolver_GetService_ReturnsNullForUnregisteredService()
        {
            var container = BuildContainer();
            var resolver = container.Resolve<IServiceResolver>();
            var service = resolver.GetService(typeof(IUnregistered));
            Assert.Null(service);
        }

        [Fact]
        public void ServiceResolver_Resolve_ReturnsRegisteredService()
        {
            var container = BuildContainer();
            var resolver = container.Resolve<IServiceResolver>();
            var service = resolver.Resolve(typeof(IService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void ServiceResolver_Dispose_DoesNotThrow()
        {
            var container = BuildContainer();
            var resolver = container.Resolve<IServiceResolver>();
            resolver.Dispose();
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void ServiceResolver_GetKeyedService_ReturnsKeyedService()
        {
            var container = BuildContainer(b => b.RegisterType<Service>().Keyed<IService>("key"));
            var resolver = container.Resolve<IServiceResolver>();
            var keyed = (IKeyedServiceProvider)resolver;
            var svc = keyed.GetKeyedService(typeof(IService), "key");
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IService>(svc);
        }

        [Fact]
        public void ServiceResolver_GetKeyedService_ReturnsNull_ForUnregisteredKey()
        {
            var container = BuildContainer(b => b.RegisterType<Service>().Keyed<IService>("key"));
            var resolver = container.Resolve<IServiceResolver>();
            var keyed = (IKeyedServiceProvider)resolver;
            var result = keyed.GetKeyedService(typeof(IService), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void ServiceResolver_GetRequiredKeyedService_ReturnsKeyedService()
        {
            var container = BuildContainer(b => b.RegisterType<Service>().Keyed<IService>("key"));
            var resolver = container.Resolve<IServiceResolver>();
            var keyed = (IKeyedServiceProvider)resolver;
            var svc = keyed.GetRequiredKeyedService(typeof(IService), "key");
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IService>(svc);
        }

        [Fact]
        public void ServiceResolver_GetRequiredKeyedService_Throws_ForUnregisteredKey()
        {
            var container = BuildContainer();
            var resolver = container.Resolve<IServiceResolver>();
            var keyed = (IKeyedServiceProvider)resolver;
            var ex = Assert.Throws<InvalidOperationException>(() => keyed.GetRequiredKeyedService(typeof(IService), "missing"));
            Assert.IsType<ComponentNotRegisteredException>(ex.InnerException);
        }
#endif

        [Fact]
        public void ConfigureDynamicProxyEngine_RegistersProxyEngineOptions()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.SourceGenerator;
                options.Strict = true;
            });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();
            var options = container.Resolve<ProxyEngineOptions>();
            Assert.Equal(ProxyEngine.SourceGenerator, options.Engine);
            Assert.True(options.Strict);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_RegistersSourceGeneratedProxyTypeGenerator()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.ConfigureDynamicProxyEngine(options => { });
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();
            var generator = container.Resolve<IProxyTypeGenerator>();
            Assert.NotNull(generator);
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_NullContainerBuilder_Throws()
        {
            ContainerBuilder builder = null;
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureDynamicProxyEngine(options => { }));
        }

        [Fact]
        public void ConfigureDynamicProxyEngine_NullConfigure_IsAllowed()
        {
            var builder = new ContainerBuilder();
            builder.RegisterDynamicProxy();
            builder.ConfigureDynamicProxyEngine(null);
            builder.RegisterType<Service>().As<IService>();
            var container = builder.Build();
            var options = container.Resolve<ProxyEngineOptions>();
            Assert.Equal(ProxyEngine.DynamicProxy, options.Engine);
        }

        public class ScopedObject
        {
        }

        public interface IUnregistered
        {
        }
    }
}
