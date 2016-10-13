using AspectCore.Lite.Generators;
using AspectCore.Lite.Test.Fakes;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using AspectCore.Lite.Abstractions;
using System.Reflection;

namespace AspectCore.Lite.Test
{
    public class ClassProxyGeneratorTest
    {
        private readonly IServiceProvider serviceProvider;

        public ClassProxyGeneratorTest()
        {
            serviceProvider = DependencyResolver.GetServiceProvider();
        }

        [Fact]
        public void ClassProxyGeneratorTest_GenerateProxyTypeWithInterceptor_Test()
        {
            var interfaceProxyGenerator = new ClassProxyGenerator(serviceProvider, typeof(TestAppServiceA));
            var proxyType = interfaceProxyGenerator.GenerateProxyType();

            var targetApp = Substitute.For<TestAppServiceA>();
            targetApp.GetAppType().Returns("mockapp");

            var proxyApp = (TestAppServiceA)ActivatorUtilities.CreateInstance(serviceProvider, proxyType, serviceProvider, targetApp);

            Assert.NotNull(proxyApp);
            Assert.NotEqual(proxyApp.GetAppType(), targetApp.GetAppType());
            Assert.Equal(proxyApp.GetAppType(), "InterceptorApp");
        }
    }
}
