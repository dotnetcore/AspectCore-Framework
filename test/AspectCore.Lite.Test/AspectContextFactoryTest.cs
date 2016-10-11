using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;

namespace AspectCore.Lite.Test.Abstractions
{
    public class AspectContextFactoryTest
    {
        private readonly IServiceProvider serviceProvider;
        public AspectContextFactoryTest()
        {
            serviceProvider = DependencyResolver.GetServiceProvider();
        }

        [Fact]
        public void AspectContextFactory_Create_Test()
        {
            var aspectContextFactory = serviceProvider.GetService<IAspectContextFactory>();
            Assert.NotNull(aspectContextFactory);
            var aspectContext = aspectContextFactory.Create();
            Assert.NotNull(aspectContext);
            Assert.Equal(serviceProvider, aspectContext.ApplicationServices);
            Assert.NotNull(aspectContext.AspectServices);
            Assert.Null(aspectContext.Parameters);
            Assert.Null(aspectContext.Proxy);
            Assert.Null(aspectContext.ReturnParameter);
            Assert.Null(aspectContext.Target);
        }
    }
}
