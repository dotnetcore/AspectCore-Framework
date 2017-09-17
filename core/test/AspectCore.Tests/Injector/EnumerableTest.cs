using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspectCore.Injector;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class EnumerableTest:InjectorTestBase
    {
        [Fact]
        public void Resolve_Enumerable()
        {
            var enumerable = ServiceResolver.Resolve<IEnumerable<IService>>();
            Assert.NotNull(enumerable);
            Assert.Equal(3, enumerable.Count());
            Assert.NotEqual(enumerable, ServiceResolver.Resolve<IEnumerable<IService>>());
        }

        [Fact]
        public void Resolve_RegisterEnumerable()
        {
            var services = new ServiceContainer();
            services.Transients.AddType<IService, Transient>();
            services.Singletons.AddType<IService, Singleton>();
            services.Scopeds.AddType<IService, Scoped>();
            services.Transients.AddDelegate<IEnumerable<IService>>(r => new IService[] { new Transient(), new Transient() , new Transient() , new Transient() });

            var resolver = services.Build();

            var enumerable = resolver.Resolve<IEnumerable<IService>>();

            Assert.NotNull(enumerable);
            Assert.Equal(4, enumerable.Count());
        }

        [Fact]
        public void Resolve_Not_Register()
        {
            var enumerable = ServiceResolver.Resolve<IEnumerable<Transient>>();
            Assert.NotNull(enumerable);
            Assert.Empty(enumerable);
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.Transients.AddType<IService, Transient>();
            serviceContainer.Singletons.AddType<IService, Singleton>();
            serviceContainer.Scopeds.AddType<IService, Scoped>();
            base.ConfigureService(serviceContainer);
        }
    }
}