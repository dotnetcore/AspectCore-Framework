using AspectCore.Extensions.Test.Fakes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Extensions.DependencyInjection.Test
{
    public class TargetInstanceProviderTest
    {
        [Fact]
        public void Scoped_Test()
        {
            var services = new ServiceCollection();

            services.AddAspectCore();

            services.AddScoped<IService, Service>();

            var aspectCoreServiceProviderFactory = new AspectCoreServiceProviderFactory();

            var proxyServiceProvider = aspectCoreServiceProviderFactory.CreateServiceProvider(services);

            var service = proxyServiceProvider.GetRequiredService<IService>();

            using (var serviceScope = proxyServiceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var socpedProvider = serviceScope.ServiceProvider;
                var service1 = socpedProvider.GetRequiredService<IService>();
                var service2 = socpedProvider.GetRequiredService<IService>();
                Assert.Equal(service1, service2);
                Assert.NotEqual(service, service1);
                Assert.NotEqual(service, service2);
            }
        }
    }
}
