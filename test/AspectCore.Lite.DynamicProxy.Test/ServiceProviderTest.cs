using AspectCore.Lite.Abstractions;
using System;
using Xunit;

namespace AspectCore.Lite.DynamicProxy.Test
{
    public class ServiceProviderTest
    {
        [Fact]
        public void ServiceProvider_Test()
        {
            IServiceProvider servicePorvider = new ProxyFactoryBuilder().Build().ServiceProvider;
            Assert.NotNull(servicePorvider.GetService(typeof(IServiceProvider)));
            Assert.NotNull(servicePorvider.GetService(typeof(IAspectValidator)));
            Assert.NotNull(servicePorvider.GetService(typeof(IProxyGenerator)));
            Assert.NotNull(servicePorvider.GetService(typeof(IAspectActivator)));
            Assert.NotNull(servicePorvider.GetService(typeof(IAspectConfiguration)));
            Assert.NotNull(servicePorvider.GetService(typeof(IAspectBuilder)));
            Assert.NotNull(servicePorvider.GetService(typeof(IInterceptorMatcher)));
            Assert.NotNull(servicePorvider.GetService(typeof(IInterceptorInjector)));
        }
    }
}
