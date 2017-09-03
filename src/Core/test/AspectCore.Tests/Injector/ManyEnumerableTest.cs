using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspectCore.Injector;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class ManyEnumerableTest : InjectorTestBase
    {
        [Fact]
        public void Resolve_Many()
        {
            var many = ServiceResolver.ResolveMany<IService>();
            Assert.NotNull(many);
            Assert.Equal(3, many.Count());
            Assert.NotEqual(many, ServiceResolver.ResolveMany<IService>());
        }

        [Fact]
        public void Resolve_ManyEnumerable()
        {
            var many = ServiceResolver.Resolve<IManyEnumerable<IService>>();
            Assert.NotNull(many);
            Assert.Equal(3, many.Count());
            Assert.NotEqual(many, ServiceResolver.Resolve<IManyEnumerable<IService>>());
        }

        [Fact]
        public void Resolve_Not_Register()
        {
            var many = ServiceResolver.ResolveMany<Transient>();
            Assert.NotNull(many);
            Assert.Equal(0, many.Count());
        }

        protected override void ConfigureService(IServiceContainer serviceContainer)
        {
            serviceContainer.Transients.AddType<IService, Transient>();
            serviceContainer.Singletons.AddType<IService, Singleton>();
            serviceContainer.Scopeds.AddType<IService, Scoped>();

            serviceContainer.Singletons.AddDelegate<IManyEnumerable<IService>>(r => new ManyEnumerable<IService>(new IService[] { new Transient(), new Transient(), new Transient(), new Transient() }));

            base.ConfigureService(serviceContainer);
        }
    }
}
