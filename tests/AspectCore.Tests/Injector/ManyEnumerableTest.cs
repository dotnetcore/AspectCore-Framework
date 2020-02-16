using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AspectCore.DependencyInjection;
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
            Assert.Empty(many);
        }
        
        [Fact]
        public void Resolve_Enumerable_Lifetime()
        {
            var many = ServiceResolver.Resolve<IEnumerable<IService>>().ToArray();
            var many1 = ServiceResolver.Resolve<IEnumerable<IService>>().ToArray();
            Assert.NotEqual(many[0], many1[0]);
            Assert.Equal(many[1], many1[1]);
            Assert.Equal(many[2], many1[2]);
            using (var scope = ServiceResolver.CreateScope())
            {
                var many2 = scope.Resolve<IEnumerable<IService>>().ToArray();
                Assert.NotEqual(many[0], many2[0]);
                Assert.Equal(many[1], many2[1]);
                Assert.NotEqual(many[2], many2[2]);
            }
        }
        
        [Fact]
        public void Resolve_Many_Lifetime()
        {
            var many = ServiceResolver.ResolveMany<IService>().ToArray();
            var many1 = ServiceResolver.ResolveMany<IService>().ToArray();
            Assert.NotEqual(many[0], many1[0]);
            Assert.Equal(many[1], many1[1]);
            Assert.Equal(many[2], many1[2]);
            using (var scope = ServiceResolver.CreateScope())
            {
                var many2 = scope.Resolve<IEnumerable<IService>>().ToArray();
                Assert.NotEqual(many[0], many2[0]);
                Assert.Equal(many[1], many2[1]);
                Assert.NotEqual(many[2], many2[2]);
            }
        }

        protected override void ConfigureService(IServiceContext serviceContext)
        {
            serviceContext.Transients.AddType<IService, Transient>();
            serviceContext.Singletons.AddType<IService, Singleton>();
            serviceContext.Scopeds.AddType<IService, Scoped>();

            serviceContext.Singletons.AddDelegate<IManyEnumerable<IService>>(r => new ManyEnumerable<IService>(new IService[] { new Transient(), new Transient(), new Transient(), new Transient() }));

            base.ConfigureService(serviceContext);
        }
    }
}
