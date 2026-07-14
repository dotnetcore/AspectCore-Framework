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
    /// Tests targeting uncovered lines in ServiceValidator (tested indirectly through
    /// WeaveDynamicProxyService).
    ///
    /// ServiceValidator.TryValidate paths covered:
    /// - IsVisible check (class service with non-visible implementation)
    /// - CanInherited check (class service with sealed implementation)
    /// - GetImplementationType keyed instance / keyed factory
    /// </summary>
    public class ServiceValidatorAndExtensionsTests
    {
        public class ValidatorInterceptor : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                context.ReturnValue = "validated";
            }
        }

        // ---- Interfaces and classes for validation tests ----

        public interface IValidatorTestService
        {
            [ValidatorInterceptor]
            string Get();
        }

        public class ValidatorTestService : IValidatorTestService
        {
            public string Get() => "validator";
        }

        // Class service type - for testing class-specific validation paths
        public class ValidatorClassService
        {
            [ValidatorInterceptor]
            public virtual string Get() => "class-validator";
        }

        // Sealed class - cannot be inherited (CanInherited check)
        public sealed class SealedValidatorServiceImpl : ValidatorClassService
        {
            public override string Get() => "sealed";
        }

        // Internal class - not visible (IsVisible check)
        internal class InternalValidatorServiceImpl : ValidatorClassService
        {
            public override string Get() => "internal";
        }

        // ---- IsVisible check: class service with non-visible implementation (lines 52-53) ----

        [Fact]
        public void WeaveDynamicProxyService_ClassServiceWithInternalImpl_NotProxied()
        {
            var services = new ServiceCollection();
            // ServiceType is a class, ImplementationType is an internal class (not visible)
            services.AddTransient<ValidatorClassService, InternalValidatorServiceImpl>();
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ValidatorClassService));
            Assert.NotNull(descriptor);
            // Should keep original implementation type (not proxied) because impl is not visible
            Assert.Equal(typeof(InternalValidatorServiceImpl), descriptor.ImplementationType);
        }

        // ---- CanInherited check: class service with sealed implementation (lines 57-58) ----

        [Fact]
        public void WeaveDynamicProxyService_ClassServiceWithSealedImpl_NotProxied()
        {
            var services = new ServiceCollection();
            // ServiceType is a class, ImplementationType is a sealed class (can't inherit)
            services.AddTransient<ValidatorClassService, SealedValidatorServiceImpl>();
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ValidatorClassService));
            Assert.NotNull(descriptor);
            // Should keep original implementation type (not proxied) because impl is sealed
            Assert.Equal(typeof(SealedValidatorServiceImpl), descriptor.ImplementationType);
        }

        // ---- GetImplementationType: keyed instance (lines 75-77) ----

        [Fact]
        public void WeaveDynamicProxyService_KeyedInterfaceWithInstance_ValidatesAndProxies()
        {
            var services = new ServiceCollection();
            var instance = new ValidatorTestService();
            services.AddKeyedSingleton<IValidatorTestService>("inst-key", instance);
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(IValidatorTestService) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            // Should be proxied because the interface has an interceptor
            Assert.NotNull(descriptor.KeyedImplementationFactory);
        }

        // ---- GetImplementationType: keyed factory (lines 79-81) ----

        [Fact]
        public void WeaveDynamicProxyService_KeyedInterfaceWithFactory_ValidatesAndProxies()
        {
            var services = new ServiceCollection();
            services.AddKeyedTransient<IValidatorTestService>("fact-key", (provider, key) => new ValidatorTestService());
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(IValidatorTestService) && x.ServiceKey != null);
            Assert.NotNull(descriptor);
            // Should be proxied because the interface has an interceptor
            Assert.NotNull(descriptor.KeyedImplementationFactory);
        }

        // ---- GetImplementationType: non-keyed instance ----

        [Fact]
        public void WeaveDynamicProxyService_InterfaceWithInstance_ValidatesAndProxies()
        {
            var services = new ServiceCollection();
            var instance = new ValidatorTestService();
            services.AddSingleton<IValidatorTestService>(instance);
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(IValidatorTestService));
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.ImplementationFactory);
        }

        // ---- GetImplementationType: non-keyed factory ----

        [Fact]
        public void WeaveDynamicProxyService_InterfaceWithFactory_ValidatesAndProxies()
        {
            var services = new ServiceCollection();
            services.AddTransient<IValidatorTestService>(provider => new ValidatorTestService());
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(IValidatorTestService));
            Assert.NotNull(descriptor);
            Assert.NotNull(descriptor.ImplementationFactory);
        }

        // ---- Class service type that IS visible and inheritable - should be proxied ----

        public class InheritableValidatorService
        {
            [ValidatorInterceptor]
            public virtual string Get() => "inheritable";
        }

        [Fact]
        public void WeaveDynamicProxyService_ClassServiceWithInheritableImpl_Proxied()
        {
            var services = new ServiceCollection();
            services.AddTransient<InheritableValidatorService>();
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(InheritableValidatorService));
            Assert.NotNull(descriptor);
            // Should be proxied because the class is visible and inheritable
            Assert.NotEqual(typeof(InheritableValidatorService), descriptor.ImplementationType);
        }

        // ---- Service with factory-based descriptor (no ImplementationType) ----

        [Fact]
        public void WeaveDynamicProxyService_FactoryBasedService_ValidatesCorrectly()
        {
            var services = new ServiceCollection();
            services.AddTransient(provider => new ValidatorTestService());
            var result = services.WeaveDynamicProxyService();
            var descriptor = result.FirstOrDefault(x => x.ServiceType == typeof(ValidatorTestService));
            Assert.NotNull(descriptor);
        }
    }
}


#endif
