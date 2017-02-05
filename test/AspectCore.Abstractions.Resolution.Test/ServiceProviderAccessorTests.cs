using AspectCore.Abstractions.Resolution.Test.Fakes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace AspectCore.Abstractions.Resolution.Test
{
    public class ServiceProviderAccessorTests
    {
        [Fact]
        public void get_ServiceProvider_Test()
        {
            var generator = new ProxyGenerator(new AspectValidator(new AspectConfigure()));
            var proxyType = generator.CreateInterfaceProxyType(typeof(ITargetService), typeof(TargetService));
            var serviceProvider = new InstanceServiceProvider(null);
            var proxyInstance = Activator.CreateInstance(proxyType, serviceProvider, new InstanceServiceProvider(new TargetService()));

            var serviceProviderAccessor = proxyInstance as IServiceProviderAccessor;
            Assert.NotNull(serviceProviderAccessor);
            Assert.Equal(serviceProviderAccessor.ServiceProvider, serviceProvider);
        }
    }
}
