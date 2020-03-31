using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Tests.Injector
{
    public class GenericTest : InjectorTestBase
    {
        [Fact]
        public void Resolve_SimpleGeneric()
        {
            var service = ServiceResolver.Resolve<ISimpleGeneric<IService>>();
            Assert.NotNull(service);
            Assert.IsType<SimpleGeneric<IService>>(service);
        }

        [Fact]
        public void Resolve_DelegateSimpleGeneric()
        {
            var service = ServiceResolver.Resolve<IDelegateSimpleGeneric<IService>>();
            Assert.NotNull(service);
            Assert.IsType<SimpleGeneric<IService>>(service);
        }

        [Fact]
        public void Resolve_InstanceSimpleGeneric()
        {
            var service = ServiceResolver.Resolve<IInstanceSimpleGeneric<IService>>();
            Assert.NotNull(service);
            Assert.IsType<SimpleGeneric<IService>>(service);
        }

        protected override void ConfigureService(IServiceContext services)
        {
            services.Transients.AddType(typeof(ISimpleGeneric<>), typeof(SimpleGeneric<>));
            services.Transients.AddDelegate(typeof(IDelegateSimpleGeneric<>), r => new SimpleGeneric<IService>());
            services.Singletons.AddInstance(typeof(IInstanceSimpleGeneric<>), new SimpleGeneric<IService>());
        }
    }
}