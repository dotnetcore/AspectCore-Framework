using System;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    /// <summary>
    /// Tests the internal DI adapter classes (MsdiServiceResolver, MsdiScopeResolverFactory,
    /// ServiceScope, ServiceScopeFactory, SupportRequiredService) through the public interfaces
    /// they implement, by resolving them from the built DI container.
    /// </summary>
    public class ServiceResolverAndScopeTests
    {
        public interface IResolverTestService
        {
            string GetName();
        }

        public class ResolverTestService : IResolverTestService
        {
            public string GetName() => "resolver";
        }

        // ---- IServiceResolver (MsdiServiceResolver) ----

        [Fact]
        public void ServiceResolver_CanBeResolved_FromDynamicProxyProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            Assert.NotNull(resolver);
        }

        [Fact]
        public void ServiceResolver_GetService_ReturnsService()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            var svc = resolver.GetService(typeof(IResolverTestService));
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IResolverTestService>(svc);
        }

        [Fact]
        public void ServiceResolver_GetService_ReturnsNull_ForUnregisteredService()
        {
            var services = new ServiceCollection();
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            var result = resolver.GetService(typeof(IResolverTestService));
            Assert.Null(result);
        }

        [Fact]
        public void ServiceResolver_Resolve_ReturnsService()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            var svc = resolver.Resolve(typeof(IResolverTestService));
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IResolverTestService>(svc);
        }

        [Fact]
        public void ServiceResolver_Dispose_DoesNotThrow()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            resolver.Dispose();
        }

#if NET8_0_OR_GREATER
        [Fact]
        public void ServiceResolver_GetKeyedService_ReturnsKeyedService()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IResolverTestService, ResolverTestService>("key");
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            var svc = resolver.GetKeyedService(typeof(IResolverTestService), "key");
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IResolverTestService>(svc);
        }

        [Fact]
        public void ServiceResolver_GetKeyedService_ReturnsNull_ForUnregisteredKey()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IResolverTestService, ResolverTestService>("key");
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            var result = resolver.GetKeyedService(typeof(IResolverTestService), "missing");
            Assert.Null(result);
        }

        [Fact]
        public void ServiceResolver_GetRequiredKeyedService_ReturnsKeyedService()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IResolverTestService, ResolverTestService>("key");
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            var svc = resolver.GetRequiredKeyedService(typeof(IResolverTestService), "key");
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IResolverTestService>(svc);
        }

        [Fact]
        public void ServiceResolver_GetRequiredKeyedService_Throws_ForUnregisteredKey()
        {
            var services = new ServiceCollection();
            var provider = services.BuildDynamicProxyProvider();
            var resolver = provider.GetRequiredService<IServiceResolver>();
            Assert.Throws<InvalidOperationException>(() => resolver.GetRequiredKeyedService(typeof(IResolverTestService), "missing"));
        }
#endif

        // ---- IScopeResolverFactory (MsdiScopeResolverFactory) ----

        [Fact]
        public void ScopeResolverFactory_CanBeResolved_FromDynamicProxyProvider()
        {
            var services = new ServiceCollection();
            var provider = services.BuildDynamicProxyProvider();
            var factory = provider.GetRequiredService<IScopeResolverFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void ScopeResolverFactory_CreateScope_ReturnsServiceResolver()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildDynamicProxyProvider();
            var factory = provider.GetRequiredService<IScopeResolverFactory>();
            var scope = factory.CreateScope();
            Assert.NotNull(scope);
            Assert.IsAssignableFrom<IServiceResolver>(scope);
        }

        [Fact]
        public void ScopeResolverFactory_CreateScope_CanResolveServices()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildDynamicProxyProvider();
            var factory = provider.GetRequiredService<IScopeResolverFactory>();
            var scope = factory.CreateScope();
            var svc = scope.Resolve(typeof(IResolverTestService));
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IResolverTestService>(svc);
        }

        [Fact]
        public void ScopeResolverFactory_CreateScope_Dispose_DoesNotThrow()
        {
            var services = new ServiceCollection();
            var provider = services.BuildDynamicProxyProvider();
            var factory = provider.GetRequiredService<IScopeResolverFactory>();
            var scope = factory.CreateScope();
            scope.Dispose();
        }

        // ---- IServiceScopeFactory (ServiceScopeFactory) ----

        [Fact]
        public void ServiceScopeFactory_CanBeResolved_FromServiceContextProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        public void ServiceScopeFactory_CreateScope_ReturnsServiceScope()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            var scope = factory.CreateScope();
            Assert.NotNull(scope);
            Assert.IsAssignableFrom<IServiceScope>(scope);
        }

        // ---- IServiceScope (ServiceScope) ----

        [Fact]
        public void ServiceScope_ServiceProvider_IsNotNull()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            var scope = factory.CreateScope();
            Assert.NotNull(scope.ServiceProvider);
        }

        [Fact]
        public void ServiceScope_ServiceProvider_CanResolveServices()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            var scope = factory.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<IResolverTestService>();
            Assert.Equal("resolver", svc.GetName());
        }

        [Fact]
        public void ServiceScope_Dispose_DoesNotThrow()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var factory = provider.GetRequiredService<IServiceScopeFactory>();
            var scope = factory.CreateScope();
            scope.Dispose();
        }

        // ---- ISupportRequiredService (SupportRequiredService) ----

        [Fact]
        public void SupportRequiredService_CanBeResolved_FromServiceContextProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var svc = provider.GetRequiredService<ISupportRequiredService>();
            Assert.NotNull(svc);
        }

        [Fact]
        public void SupportRequiredService_GetRequiredService_ReturnsService()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var requiredSvc = provider.GetRequiredService<ISupportRequiredService>();
            var svc = requiredSvc.GetRequiredService(typeof(IResolverTestService));
            Assert.NotNull(svc);
            Assert.IsAssignableFrom<IResolverTestService>(svc);
        }

        [Fact]
        public void SupportRequiredService_GetRequiredService_NullType_Throws()
        {
            var services = new ServiceCollection();
            services.AddTransient<IResolverTestService, ResolverTestService>();
            var provider = services.BuildServiceContextProvider();
            var requiredSvc = provider.GetRequiredService<ISupportRequiredService>();
            Assert.Throws<ArgumentNullException>(() => requiredSvc.GetRequiredService(null));
        }
    }
}
