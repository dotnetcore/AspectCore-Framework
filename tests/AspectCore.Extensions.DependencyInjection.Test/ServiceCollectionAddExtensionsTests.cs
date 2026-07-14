using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class ServiceCollectionAddExtensionsTests
    {
        public interface IAddTestService
        {
            string GetValue();
        }

        public class AddTestService : IAddTestService
        {
            public string GetValue() => "value";
        }

        public class AddTestInterceptor : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                context.ReturnValue = "intercepted";
            }
        }

        // ---- Null argument checks ----

        [Fact]
        public void AddInterfaceProxy_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.AddInterfaceProxy(typeof(IAddTestService), ServiceLifetime.Transient));
        }

        [Fact]
        public void AddInterfaceProxy_NullInterfaceType_Throws()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentNullException>(() => services.AddInterfaceProxy(null, ServiceLifetime.Transient));
        }

        [Fact]
        public void AddInterfaceProxy_NonInterfaceType_Throws()
        {
            var services = new ServiceCollection();
            Assert.Throws<ArgumentException>(() => services.AddInterfaceProxy(typeof(string), ServiceLifetime.Transient));
        }

        // ---- Type-based overloads ----

        [Fact]
        public void AddInterfaceProxy_RegistersServiceWithCorrectLifetime()
        {
            var services = new ServiceCollection();
            services.AddInterfaceProxy(typeof(IAddTestService), ServiceLifetime.Singleton);
            var descriptor = Assert.Single(services);
            Assert.Equal(typeof(IAddTestService), descriptor.ServiceType);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
            Assert.NotNull(descriptor.ImplementationFactory);
        }

        [Fact]
        public void AddTransientInterfaceProxy_RegistersTransient()
        {
            var services = new ServiceCollection();
            services.AddTransientInterfaceProxy(typeof(IAddTestService));
            var descriptor = Assert.Single(services);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void AddScopedInterfaceProxy_RegistersScoped()
        {
            var services = new ServiceCollection();
            services.AddScopedInterfaceProxy(typeof(IAddTestService));
            var descriptor = Assert.Single(services);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void AddSingletonInterfaceProxy_RegistersSingleton()
        {
            var services = new ServiceCollection();
            services.AddSingletonInterfaceProxy(typeof(IAddTestService));
            var descriptor = Assert.Single(services);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        // ---- Generic overloads ----

        [Fact]
        public void AddInterfaceProxy_Generic_RegistersService()
        {
            var services = new ServiceCollection();
            services.AddInterfaceProxy<IAddTestService>(ServiceLifetime.Scoped);
            var descriptor = Assert.Single(services);
            Assert.Equal(typeof(IAddTestService), descriptor.ServiceType);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void AddTransientInterfaceProxy_Generic_RegistersTransient()
        {
            var services = new ServiceCollection();
            services.AddTransientInterfaceProxy<IAddTestService>();
            var descriptor = Assert.Single(services);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
        }

        [Fact]
        public void AddScopedInterfaceProxy_Generic_RegistersScoped()
        {
            var services = new ServiceCollection();
            services.AddScopedInterfaceProxy<IAddTestService>();
            var descriptor = Assert.Single(services);
            Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
        }

        [Fact]
        public void AddSingletonInterfaceProxy_Generic_RegistersSingleton()
        {
            var services = new ServiceCollection();
            services.AddSingletonInterfaceProxy<IAddTestService>();
            var descriptor = Assert.Single(services);
            Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
        }

        // ---- Functional: resolve and use the proxy ----

        [Fact]
        public void AddInterfaceProxy_ResolvedProxy_InterceptsMethod()
        {
            var services = new ServiceCollection();
            services.AddTransient<IAddTestService, AddTestService>();
            services.ConfigureDynamicProxy(config =>
            {
                config.Interceptors.AddDelegate((ctx, next) =>
                {
                    ctx.ReturnValue = "intercepted";
                    return next(ctx);
                });
            });
            services.AddInterfaceProxy<IAddTestService>(ServiceLifetime.Transient);

            var provider = services.BuildDynamicProxyProvider();
            var proxy = provider.GetRequiredService<IAddTestService>();
            Assert.NotNull(proxy);
        }
    }
}
