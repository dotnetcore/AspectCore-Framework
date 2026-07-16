using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

#if NET8_0_OR_GREATER
namespace AspectCore.Extensions.DependencyInjection.Test
{
    /// <summary>
    /// Tests targeting remaining uncovered lines in ServiceValidator:
    /// - Line 52: return false when implementation type is not a class and not factory-registered
    /// Tests the validator directly via reflection since WeaveDynamicProxyService
    /// builds a service provider internally which fails on invalid descriptors.
    /// </summary>
    public class ServiceValidatorCoverageTests
    {
        public class CoverageInterceptor : AbstractInterceptorAttribute
        {
            public override async Task Invoke(AspectContext context, AspectDelegate next)
            {
                await context.Invoke(next);
                context.ReturnValue = "covered";
            }
        }

        public interface ICoverageTestService
        {
            [CoverageInterceptor]
            string Get();
        }

        public class CoverageTestService : ICoverageTestService
        {
            public string Get() => "test";
        }

        public interface IStructImplService
        {
            [CoverageInterceptor]
            int Get();
        }

        public struct StructImpl : IStructImplService
        {
            public int Get() => 42;
        }

        private static object CreateValidator()
        {
            // Use TryAddDynamicProxyServices (internal) to register all needed services
            var services = new ServiceCollection();
            var extType = typeof(ServiceCollectionBuildExtensions).Assembly
                .GetType("AspectCore.Extensions.DependencyInjection.ServiceCollectionExtensions");
            var tryAddMethod = extType.GetMethod("TryAddDynamicProxyServices", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.NotNull(tryAddMethod);
            tryAddMethod.Invoke(null, new object[] { services });

            var provider = services.BuildServiceProvider();
            var validatorBuilder = provider.GetRequiredService<IAspectValidatorBuilder>();

            var validatorType = typeof(ServiceCollectionBuildExtensions).Assembly
                .GetType("AspectCore.Extensions.DependencyInjection.ServiceValidator");
            Assert.NotNull(validatorType);

            var ctor = validatorType.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new[] { typeof(IAspectValidatorBuilder) },
                null);
            Assert.NotNull(ctor);

            var instance = ctor.Invoke(new object[] { validatorBuilder });
            provider.Dispose();
            return instance;
        }

        private static bool TryValidate(object validator, ServiceDescriptor descriptor, out Type implementationType)
        {
            var validatorType = validator.GetType();
            var method = validatorType.GetMethod("TryValidate", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);

            var args = new object[] { descriptor, null };
            var result = (bool)method.Invoke(validator, args);
            implementationType = args[1] as Type;
            return result;
        }

        [Fact]
        public void TryValidate_InterfaceAsImplementation_ReturnsFalse()
        {
            var validator = CreateValidator();

            // Implementation type is ICoverageTestService (an interface, not a class)
            // Not factory-registered, so validator returns false (line 52)
            var descriptor = ServiceDescriptor.Describe(
                typeof(ICoverageTestService),
                typeof(ICoverageTestService),
                ServiceLifetime.Transient);

            var result = TryValidate(validator, descriptor, out var implType);
            Assert.False(result);
        }

        [Fact]
        public void TryValidate_StructAsImplementation_ReturnsFalse()
        {
            var validator = CreateValidator();

            // Implementation type is StructImpl (a struct, not a class)
            // Not factory-registered, so validator returns false (line 52)
            var descriptor = ServiceDescriptor.Describe(
                typeof(IStructImplService),
                typeof(StructImpl),
                ServiceLifetime.Transient);

            var result = TryValidate(validator, descriptor, out var implType);
            Assert.False(result);
        }
    }
}
#endif
