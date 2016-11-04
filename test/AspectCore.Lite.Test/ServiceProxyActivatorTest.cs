using AspectCore.Lite.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AspectCore.Lite.Test
{
    public class ServiceProxyActivatorTest:IDependencyInjection
    {
        [Fact]
        public void ServiceProxyActivator_Test()
        {
            var serviceProvider = this.BuildServiceProvider();
            var proxyActivator = serviceProvider.GetRequiredService<IProxyActivator>();
            Assert.IsAssignableFrom<IProxyActivator>(proxyActivator);
        }
    }
}