using System.Reflection;
using AspectCore.Abstractions;
using AspectCore.Extensions.Test.Fakes;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class AspectCoreServiceProviderFactoryTests
    {
        [Fact]
        public void CreateBuilderWithProxy_Test()
        {
            var services = new ServiceCollection();
            services.AddAspectCore();
            services.AddTransient<IService, Service>();
            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();
            var proxyServices = aspectCoreServiceProviderFactory.CreateBuilder(services);
            var descriptor = Assert.Single(proxyServices, d => d.ServiceType == typeof(IService));
            Assert.NotNull(descriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, descriptor.Lifetime);
            Assert.IsNotType<Service>(descriptor.ImplementationType);
            Assert.True(descriptor.ImplementationType.GetTypeInfo().IsDefined(typeof(DynamicallyAttribute)));
        }
    }
}