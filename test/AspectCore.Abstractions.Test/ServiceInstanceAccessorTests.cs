using System;
using AspectCore.Abstractions.Internal.Test.Fakes;
using AspectCore.Abstractions.Test.Fakes;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class ServiceInstanceAccessorTests
    {
        [Fact]
        public void get_ServiceInstance_Test()
        {
            var generator = new ProxyGenerator(AspectValidatorFactory.GetAspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            var serviceInsatnce = new TargetService();
            var proxyInstance = Activator.CreateInstance(proxyType, new InstanceServiceProvider(null), new InstanceServiceProvider(serviceInsatnce));

            var serviceInstanceAccessor = proxyInstance as IServiceInstanceAccessor;
            Assert.NotNull(serviceInstanceAccessor);
            Assert.Equal(serviceInstanceAccessor.ServiceInstance, serviceInsatnce);
        }


        [Fact]
        public void get_ServiceInstance_With_Generic_Test()
        {
            var generator = new ProxyGenerator(AspectValidatorFactory.GetAspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            var serviceInsatnce = new TargetService();
            var proxyInstance = Activator.CreateInstance(proxyType, new InstanceServiceProvider(null), new InstanceServiceProvider(serviceInsatnce));

            var serviceInstanceAccessor = proxyInstance as IServiceInstanceAccessor<ITargetService>;
            Assert.NotNull(serviceInstanceAccessor);
            Assert.Equal<ITargetService>(serviceInstanceAccessor.ServiceInstance, serviceInsatnce);
        }
    }
}
