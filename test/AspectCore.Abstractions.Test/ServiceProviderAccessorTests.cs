using System;
using AspectCore.Abstractions.Internal.Test.Fakes;
using AspectCore.Abstractions.Test.Fakes;
using Xunit;

namespace AspectCore.Abstractions.Internal.Test
{
    public class ServiceProviderAccessorTests
    {
        [Fact]
        public void get_ServiceProvider_Test()
        {
            var generator = new ProxyGenerator(AspectValidatorFactory.GetAspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            var serviceProvider = new InstanceServiceProvider(null);
            var proxyInstance = Activator.CreateInstance(proxyType, serviceProvider, new InstanceServiceProvider(new TargetService()));

            var serviceProviderAccessor = proxyInstance as IServiceProviderAccessor;
            Assert.NotNull(serviceProviderAccessor);
            Assert.Equal(serviceProviderAccessor.ServiceProvider, serviceProvider);
        }
    }
}
