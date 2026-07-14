using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

#if NET8_0_OR_GREATER
namespace AspectCore.Extensions.DependencyInjection.Test
{
    /// <summary>
    /// Tests targeting uncovered lines in ServiceCollectionBuildExtensions:
    /// - BuildAspectCoreServiceProvider (obsolete)
    /// - MakeProxyService keyed class / generic paths
    /// - CreateFactory ImplementationInstance / ImplementationFactory paths (2-param ctor)
    /// - CreateKeyedFactory ImplementationInstance / ImplementationFactory paths (2-param ctor)
    /// </summary>
    public class ServiceCollectionBuildExtensionsCoverageTests
    {
        // ---- Test service types ----

        public interface ICovTestService
        {
            string Get();
        }

        public class CovTestServiceImpl : ICovTestService
        {
            public string Get() => "cov";
        }

        public class CovTestInterceptor : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                context.ReturnValue = "intercepted-cov";
            }
        }

        public interface ICovTestServiceWithInterceptor
        {
            [CovTestInterceptor]
            string Get();
        }

        public class CovTestServiceWithInterceptorImpl : ICovTestServiceWithInterceptor
        {
            public string Get() => "with-interceptor";
        }

        // ---- MakeProxyService - Keyed class service (lines 70-80) ----

        public class CovClassService
        {
            [CovTestInterceptor]
            public virtual string Get() => "class-service";
        }

        [Fact]
        public void WeaveDynamicProxyService_KeyedClassService_ProxiesClass()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<CovClassService>("class-key");
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(CovClassService) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            // The proxy type should be different from the original implementation type
            Assert.NotEqual(typeof(CovClassService), descriptor.ImplementationType);
        }

        // ---- CreateFactory - ImplementationInstance (2-param ctor, lines 183-186) ----

        [Fact]
        public void WeaveDynamicProxyService_InterfaceWithImplementationInstance_ProxiesInterface()
        {
            var services = new ServiceCollection();
            var instance = new CovTestServiceWithInterceptorImpl();
            services.AddSingleton<ICovTestServiceWithInterceptor>(instance);
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ICovTestServiceWithInterceptor));
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.ImplementationFactory);
        }

        // ---- CreateFactory - ImplementationFactory (2-param ctor, lines 189-192) ----

        [Fact]
        public void WeaveDynamicProxyService_InterfaceWithImplementationFactory_ProxiesInterface()
        {
            var services = new ServiceCollection();
            services.AddTransient<ICovTestServiceWithInterceptor>(provider => new CovTestServiceWithInterceptorImpl());
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ICovTestServiceWithInterceptor));
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.ImplementationFactory);
        }

        // ---- CreateKeyedFactory - Keyed interface with ImplementationInstance (2-param ctor, lines 254-257) ----

        [Fact]
        public void WeaveDynamicProxyService_KeyedInterfaceWithImplementationInstance_ProxiesInterface()
        {
            var services = new ServiceCollection();
            var instance = new CovTestServiceWithInterceptorImpl();
            services.AddKeyedSingleton<ICovTestServiceWithInterceptor>("inst-key", instance);
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ICovTestServiceWithInterceptor) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.KeyedImplementationFactory);
        }

        // ---- CreateKeyedFactory - Keyed interface with ImplementationFactory (2-param ctor, lines 260-263) ----

        [Fact]
        public void WeaveDynamicProxyService_KeyedInterfaceWithImplementationFactory_ProxiesInterface()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<ICovTestServiceWithInterceptor>("factory-key", (provider, key) => new CovTestServiceWithInterceptorImpl());
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ICovTestServiceWithInterceptor) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.KeyedImplementationFactory);
        }

        // ---- CreateKeyedFactory - Keyed interface with ImplementationType (2-param ctor, lines 267-274 already covered, verify end-to-end) ----

        [Fact]
        public void WeaveDynamicProxyService_KeyedInterfaceWithImplementationType_ResolvesAndIntercepts()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<ICovTestServiceWithInterceptor, CovTestServiceWithInterceptorImpl>("type-key");
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ICovTestServiceWithInterceptor) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.KeyedImplementationFactory);

            // Verify the proxy actually works
            var provider = result.BuildServiceProvider();
            var svc = provider.GetKeyedService<ICovTestServiceWithInterceptor>("type-key");
            Assert.NotNull(svc);
            Assert.Equal("intercepted-cov", svc.Get());
        }

        // ---- MakeProxyService - Non-keyed class service (verifies class path works) ----

        [Fact]
        public void WeaveDynamicProxyService_ClassService_ProxiesClass()
        {
            var services = new ServiceCollection();
            services.AddTransient<CovClassService>();
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(CovClassService));
            Assert.NotNull(descriptor);
            Assert.NotEqual(typeof(CovClassService), descriptor.ImplementationType);
        }
    }
}


#endif
