using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class InternalServiceTest : InjectorTestBase
    {
        [Fact]
        public void Resolve_IServiceResolver()
        {
            var serviceResolver = ServiceResolver.Resolve<IServiceResolver>();
            Assert.Equal(ServiceResolver, serviceResolver);
            using(var scopedServiceResolver = ServiceResolver.CreateScope())
            {
                Assert.Equal(scopedServiceResolver, scopedServiceResolver.Resolve<IServiceResolver>());
            }
        }

        [Fact]
        public void Resolve_IServiceProvider()
        {
            var serviceResolver = ServiceResolver.Resolve<IServiceProvider>();
            Assert.Equal(ServiceResolver, serviceResolver);
            using (var scopedServiceResolver = ServiceResolver.CreateScope())
            {
                Assert.Equal(scopedServiceResolver, scopedServiceResolver.Resolve<IServiceProvider>());
            }
        }

        [Fact]
        public void Replace_IServiceProvider()
        {
            var services = new ServiceContext();
            services.Transients.AddType<IServiceProvider, ServiceProvider>();
            var resolver = services.Build();
            var serviceProvider = resolver.Resolve<IServiceProvider>();
            Assert.NotEqual(resolver, serviceProvider);
            Assert.IsType<ServiceProvider>(serviceProvider);
        }

        public class ServiceProvider : IServiceProvider
        {
            private readonly IServiceResolver _serviceResolver;

            public ServiceProvider(IServiceResolver serviceResolver)
            {
                _serviceResolver = serviceResolver;
            }

            public object GetService(Type serviceType)
            {
                return _serviceResolver.Resolve(serviceType);
            }
        }
    }
}
