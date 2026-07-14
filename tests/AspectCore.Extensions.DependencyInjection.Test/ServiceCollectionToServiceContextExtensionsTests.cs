using System;
using System.Linq;
using AspectCore.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

#if NET8_0_OR_GREATER
namespace AspectCore.Extensions.DependencyInjection.Test
{
    /// <summary>
    /// Tests targeting uncovered lines in ServiceCollectionToServiceContextExtensions:
    /// - ToServiceContext null check
    /// - AddAspectServiceContext null check
    /// - Replace method: keyed type/instance/factory, non-keyed instance/factory
    /// </summary>
    public class ServiceCollectionToServiceContextExtensionsTests
    {
        public interface IToCtxTestService
        {
            string GetValue();
        }

        public class ToCtxTestService : IToCtxTestService
        {
            public string GetValue() => "toctx";
        }

        // ---- ToServiceContext null check (lines 26-27) ----

        [Fact]
        public void ToServiceContext_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.ToServiceContext());
        }

        // ---- AddAspectServiceContext null check (lines 38-39) ----

        [Fact]
        public void AddAspectServiceContext_NullServices_Throws()
        {
            IServiceCollection services = null;
            Assert.Throws<ArgumentNullException>(() => services.AddAspectServiceContext());
        }

        // ---- Replace - Keyed type implementation (lines 50-53) ----

        [Fact]
        public void ToServiceContext_KeyedTypeImplementation_ConvertsToServiceDefinition()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IToCtxTestService, ToCtxTestService>("key1");
            var context = services.ToServiceContext();
            Assert.NotNull(context);
        }

        // ---- Replace - Keyed instance implementation (lines 55-57) ----

        [Fact]
        public void ToServiceContext_KeyedInstanceImplementation_ConvertsToServiceDefinition()
        {
            var services = new ServiceCollection();
            var instance = new ToCtxTestService();
            services.AddKeyedSingleton<IToCtxTestService>("key2", instance);
            var context = services.ToServiceContext();
            Assert.NotNull(context);
        }

        // ---- Replace - Keyed factory implementation (lines 60-61) ----

        [Fact]
        public void ToServiceContext_KeyedFactoryImplementation_ConvertsToServiceDefinition()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IToCtxTestService>("key3", (provider, key) => new ToCtxTestService());
            var context = services.ToServiceContext();
            Assert.NotNull(context);
        }

        // ---- Replace - Non-keyed instance implementation (lines 69-71) ----

        [Fact]
        public void ToServiceContext_NonKeyedInstanceImplementation_ConvertsToServiceDefinition()
        {
            var services = new ServiceCollection();
            var instance = new ToCtxTestService();
            services.AddSingleton<IToCtxTestService>(instance);
            var context = services.ToServiceContext();
            Assert.NotNull(context);
        }

        // ---- Replace - Non-keyed factory implementation (lines 74-75) ----

        [Fact]
        public void ToServiceContext_NonKeyedFactoryImplementation_ConvertsToServiceDefinition()
        {
            var services = new ServiceCollection();
            services.AddTransient<IToCtxTestService>(provider => new ToCtxTestService());
            var context = services.ToServiceContext();
            Assert.NotNull(context);
        }

        // ---- BuildServiceContextProvider (with and without additional) ----

        [Fact]
        public void BuildServiceContextProvider_ReturnsServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddTransient<IToCtxTestService, ToCtxTestService>();
            var provider = services.BuildServiceContextProvider();
            Assert.NotNull(provider);
            var svc = provider.GetRequiredService<IToCtxTestService>();
            Assert.Equal("toctx", svc.GetValue());
        }

        [Fact]
        public void BuildServiceContextProvider_WithAdditional_InvokesCallback()
        {
            var services = new ServiceCollection();
            services.AddTransient<IToCtxTestService, ToCtxTestService>();
            bool called = false;
            var provider = services.BuildServiceContextProvider(ctx =>
            {
                called = true;
            });
            Assert.NotNull(provider);
            Assert.True(called);
        }

        // ---- ToServiceContext with all registration types combined ----

        [Fact]
        public void ToServiceContext_WithAllRegistrationTypes_Works()
        {
            var services = new ServiceCollection();
            services.AddTransient<IToCtxTestService, ToCtxTestService>();
            services.AddSingleton<IToCtxTestService>(new ToCtxTestService());
            services.AddTransient<IToCtxTestService>(p => new ToCtxTestService());
            services.AddKeyedTransient<IToCtxTestService, ToCtxTestService>("k1");
            services.AddKeyedSingleton<IToCtxTestService>("k2", new ToCtxTestService());
            services.AddKeyedTransient<IToCtxTestService>("k3", (p, k) => new ToCtxTestService());
            var context = services.ToServiceContext();
            Assert.NotNull(context);
            var provider = context.Build();
            Assert.NotNull(provider);
        }

        // ---- AddAspectServiceContext registers ServiceContextProviderFactory ----

        [Fact]
        public void AddAspectServiceContext_RegistersServiceContextProviderFactory()
        {
            var services = new ServiceCollection();
            services.AddAspectServiceContext();
            var descriptor = services.FirstOrDefault(x => x.ServiceType == typeof(IServiceProviderFactory<IServiceContext>));
            Assert.NotNull(descriptor);
            Assert.Equal(typeof(ServiceContextProviderFactory), descriptor.ImplementationType);
        }
    }
}


#endif
