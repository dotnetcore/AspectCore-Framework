using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#if NET8_0_OR_GREATER
namespace AspectCore.Extensions.DependencyInjection.Test
{
    /// <summary>
    /// Tests that use a custom ISourceGeneratedProxyRegistry to return a proxy type
    /// with a 3-parameter constructor (IAspectActivatorFactory, IServiceProvider, serviceType),
    /// covering the 3-param ctor paths in CreateFactory and CreateKeyedFactory.
    /// </summary>
    public class SourceGeneratorEngineCoverageTests
    {
        // ---- Interceptor ----

        public class SgTestInterceptor : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                context.ReturnValue = "sg-intercepted";
            }
        }

        // ---- Test service types ----

        public interface ISgTestService
        {
            [SgTestInterceptor]
            string Get();
        }

        public class SgTestServiceImpl : ISgTestService
        {
            public string Get() => "sg-original";
        }

        // ---- Custom proxy type with 3-param constructor ----
        // This simulates a source-generated proxy that has the
        // (IAspectActivatorFactory, IServiceProvider, serviceType) constructor.

        public class SgCustomProxyType : ISgTestService
        {
            private readonly IAspectActivatorFactory _activatorFactory;
            private readonly IServiceProvider _serviceProvider;
            private readonly ISgTestService _implementation;

            public SgCustomProxyType(IAspectActivatorFactory activatorFactory, IServiceProvider serviceProvider, ISgTestService implementation)
            {
                _activatorFactory = activatorFactory;
                _serviceProvider = serviceProvider;
                _implementation = implementation;
            }

            public string Get() => _implementation.Get();
        }

        // ---- Custom registry that returns the custom proxy type ----

        public class SgCustomProxyRegistry : ISourceGeneratedProxyRegistry
        {
            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                proxyType = typeof(SgCustomProxyType);
                return true;
            }
        }

        private IServiceCollection CreateServicesWithSgEngine()
        {
            var services = new ServiceCollection();
            services.AddSourceGeneratedProxyRegistry<SgCustomProxyRegistry>();
            services.ConfigureDynamicProxyEngine(options =>
            {
                options.Engine = ProxyEngine.Auto;
            });
            return services;
        }

        // ---- CreateFactory 3-param ImplementationInstance (lines 148-155) ----

        [Fact]
        public void WeaveDynamicProxyService_SgEngine_InterfaceWithInstance_Uses3ParamCtor()
        {
            var services = CreateServicesWithSgEngine();
            var instance = new SgTestServiceImpl();
            services.AddSingleton<ISgTestService>(instance);
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ISgTestService));
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.ImplementationFactory);
        }

        // ---- CreateFactory 3-param ImplementationType (lines 165-172) ----

        [Fact]
        public void WeaveDynamicProxyService_SgEngine_InterfaceWithType_Uses3ParamCtor()
        {
            var services = CreateServicesWithSgEngine();
            services.AddTransient<ISgTestService, SgTestServiceImpl>();
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ISgTestService));
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.ImplementationFactory);

            // Verify end-to-end: the proxy factory works
            var provider = result.BuildServiceProvider();
            var svc = provider.GetRequiredService<ISgTestService>();
            Assert.NotNull(svc);
        }

        // ---- CreateKeyedFactory 3-param KeyedImplementationInstance (lines 219-226) ----

        [Fact]
        public void WeaveDynamicProxyService_SgEngine_KeyedInterfaceWithInstance_Uses3ParamCtor()
        {
            var services = CreateServicesWithSgEngine();
            var instance = new SgTestServiceImpl();
            services.AddKeyedSingleton<ISgTestService>("sg-inst-key", instance);
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ISgTestService) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.KeyedImplementationFactory);
        }

        // ---- CreateKeyedFactory 3-param KeyedImplementationType (lines 236-243) ----

        [Fact]
        public void WeaveDynamicProxyService_SgEngine_KeyedInterfaceWithType_Uses3ParamCtor()
        {
            var services = CreateServicesWithSgEngine();
            services.AddKeyedTransient<ISgTestService, SgTestServiceImpl>("sg-type-key");
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ISgTestService) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.KeyedImplementationFactory);

            // Verify end-to-end
            var provider = result.BuildServiceProvider();
            var svc = provider.GetKeyedService<ISgTestService>("sg-type-key");
            Assert.NotNull(svc);
        }

        // ---- Verify all registration types with SG engine ----

        [Fact]
        public void WeaveDynamicProxyService_SgEngine_AllRegistrationTypes_Work()
        {
            var services = CreateServicesWithSgEngine();
            // Type
            services.AddTransient<ISgTestService, SgTestServiceImpl>();
            // Instance
            services.AddSingleton<ISgTestService>(new SgTestServiceImpl());
            // Keyed type
            services.AddKeyedTransient<ISgTestService, SgTestServiceImpl>("k1");
            // Keyed instance
            services.AddKeyedSingleton<ISgTestService>("k2", new SgTestServiceImpl());

            var result = services.WeaveDynamicProxyService();
            Assert.NotNull(result);

            var provider = result.BuildServiceProvider();
            var svc = provider.GetRequiredService<ISgTestService>();
            Assert.NotNull(svc);
        }
    }
}


#endif
