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

        [Fact]
        public void Intercept_OutWithDecimalParamter_Test()
        {
            var service = ServiceResolver.Resolve<IFakeServiceWithOut>();
            decimal num;
            Assert.True(service.OutDecimal(out num));
            Assert.Equal(1.0M, num);
        }

        [Fact]
        public void Intercept_OutWithIntParamter_Test()
        {
            var service = ServiceResolver.Resolve<IFakeServiceWithOut>();
            int num;
            Assert.True(service.OutInt(out num));
            Assert.Equal(1, num);
        }

        protected override void ConfigureService(IServiceContext services)
        {
            services.Transients.AddType(typeof(ISimpleGeneric<>), typeof(SimpleGeneric<>));
            services.Transients.AddDelegate(typeof(IDelegateSimpleGeneric<>), r => new SimpleGeneric<IService>());
            services.Singletons.AddInstance(typeof(IInstanceSimpleGeneric<>), new SimpleGeneric<IService>());
            services.Transients.AddType<IFakeServiceWithOut, FakeServiceWithOut>();
        }
    }
}