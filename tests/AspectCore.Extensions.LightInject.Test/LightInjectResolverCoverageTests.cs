using System;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.Parameters;
using AspectCore.Extensions.LightInject;
using AspectCoreTest.LightInject.Fakes;
using LightInject;
using Xunit;

namespace AspectCoreTest.LightInject
{
    public class LightInjectResolverCoverageTests
    {
        private static IServiceContainer CreateBuilder()
        {
            return new ServiceContainer().RegisterDynamicProxy();
        }

        [Fact]
        public void Resolve_ReturnsService_WhenRegistered()
        {
            var container = CreateBuilder();
            container.Register<IService, Service>();
            var resolver = container.GetInstance<IServiceResolver>();
            var service = resolver.Resolve(typeof(IService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void Resolve_ReturnsNull_WhenNotRegistered()
        {
            var container = CreateBuilder();
            var resolver = container.GetInstance<IServiceResolver>();
            var service = resolver.Resolve(typeof(INotRegisteredService));
            Assert.Null(service);
        }

        [Fact]
        public void Dispose_DoesNotThrow()
        {
            var container = CreateBuilder();
            var resolver = container.GetInstance<IServiceResolver>();
            resolver.Dispose();
        }

        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            var container = CreateBuilder();
            var resolver = container.GetInstance<IServiceResolver>();
            resolver.Dispose();
            resolver.Dispose();
        }

        [Fact]
        public void Resolve_ReturnsProxy_WhenServiceHasInterceptor()
        {
            var container = new ServiceContainer().RegisterDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate(next => ctx => next(ctx), Predicates.ForNameSpace("AspectCore.Extensions.Test.Fakes"));
            });
            container.Register<IService, Service>();
            var resolver = container.GetInstance<IServiceResolver>();
            var service = resolver.Resolve(typeof(IService));
            Assert.NotNull(service);
            Assert.IsAssignableFrom<IService>(service);
        }

        [Fact]
        public void ScopeResolverFactory_CreateScope_ReturnsResolver()
        {
            var container = CreateBuilder();
            container.Register<IService, Service>();
            var factory = container.GetInstance<IScopeResolverFactory>();
            using (var scope = factory.CreateScope())
            {
                Assert.NotNull(scope);
                var service = scope.Resolve(typeof(IService));
                Assert.NotNull(service);
            }
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersManyEnumerable()
        {
            var container = CreateBuilder();
            container.Register<IService, Service>();
            var many = container.GetInstance<IManyEnumerable<IService>>();
            Assert.NotNull(many);
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersAspectContextFactory()
        {
            var container = CreateBuilder();
            var factory = container.GetInstance<IAspectContextFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersAspectCachingProvider()
        {
            var container = CreateBuilder();
            var provider = container.GetInstance<IAspectCachingProvider>();
            Assert.NotNull(provider);
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersParameterInterceptorSelector()
        {
            var container = CreateBuilder();
            var selector = container.GetInstance<IParameterInterceptorSelector>();
            Assert.NotNull(selector);
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersPropertyInjectorFactory()
        {
            var container = CreateBuilder();
            var factory = container.GetInstance<IPropertyInjectorFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void RegisterDynamicProxy_RegistersAdditionalInterceptorSelector()
        {
            var container = CreateBuilder();
            var selector = container.GetInstance<IAdditionalInterceptorSelector>();
            Assert.NotNull(selector);
        }

        public interface INotRegisteredService { }
    }
}
