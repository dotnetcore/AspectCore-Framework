using System;
using System.Collections.Generic;
using System.Text;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Tests.DynamicProxy
{
    public class ProxyGeneratorTest: DynamicProxyTestBase
    {
        [Fact]
        public void CreateInterfaceProxy_Without_ImplType()
        {
            var serviceProxy = ProxyGenerator.CreateInterfaceProxy<IService>();
            Assert.NotNull(serviceProxy);
            Assert.IsAssignableFrom<IService>(serviceProxy);
            Assert.Equal(default(Guid), serviceProxy.Id);
            var id = Guid.NewGuid();
            serviceProxy.Id = id;
            Assert.Equal(id, serviceProxy.Id);
            Assert.Equal(default(ILogger), serviceProxy.Logger);
        }

        protected override void Configure(IAspectConfiguration configuration)
        {
        }
    }
}
