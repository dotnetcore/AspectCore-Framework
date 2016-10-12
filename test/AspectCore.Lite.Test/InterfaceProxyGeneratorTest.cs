using AspectCore.Lite.Generators;
using AspectCore.Lite.Test.Fakes;
using Microsoft.AspNetCore.Testing;
using NSubstitute;
using System;
using Xunit;
using Xunit.Abstractions;

namespace AspectCore.Lite.Test.Generators
{
    public class InterfaceProxyGeneratorTest
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ITestOutputHelper output;

        public InterfaceProxyGeneratorTest(ITestOutputHelper output)
        {
            this.output = output;
            serviceProvider = DependencyResolver.GetServiceProvider();
        }

        [Fact]
        public void InterfaceProxyGenerator_Constructor_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IAppService));
            Assert.NotNull(interfaceProxyGenerator);
            ExceptionAssert.ThrowsArgumentNull(() => new InterfaceProxyGenerator(null, typeof(IAppService)), "serviceProvider");
            ExceptionAssert.ThrowsArgumentNull(() => new InterfaceProxyGenerator(serviceProvider, null), "interfaceType");
            ExceptionAssert.ThrowsArgument(() => new InterfaceProxyGenerator(serviceProvider, typeof(object)), "interfaceType", "Type should be interface.");
        }

        [Fact]
        public void InterfaceProxyGenerator_TypeBuilder_Throw_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IAppService));
            ExceptionAssert.Throws<InvalidOperationException>(() => interfaceProxyGenerator.TypeBuilder, $"The proxy of {typeof(IAppService).FullName} is not generated.");
        }

        [Fact]
        public void InterfaceProxyGenerator_GenerateProxyTypeMock_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IAppServiceA));
            var proxyType = interfaceProxyGenerator.GenerateProxyType();

            var targetApp = Substitute.For<IAppServiceA>();
            targetApp.AppId.Returns(1);
            targetApp.AppName.Returns("testApp");
            targetApp.RunApp(null).Returns(true);
            targetApp.GetAppType().Returns("mockapp");

            var proxyApp = (IAppServiceA)Activator.CreateInstance(proxyType, serviceProvider, targetApp);

            Assert.NotNull(proxyApp);
            Assert.Equal(proxyApp.AppId, targetApp.AppId);
            Assert.Equal(proxyApp.AppName, targetApp.AppName);
            Assert.Equal(proxyApp.GetAppType(), targetApp.GetAppType());
            Assert.Equal(proxyApp.RunApp(null), targetApp.RunApp(null));
        }

        [Fact]
        public void InterfaceProxyGenerator_GenerateProxyType_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IAppServiceA));
            var proxyType = interfaceProxyGenerator.GenerateProxyType();

            var targetApp = new TestAppServiceA();

            var proxyApp = (IAppServiceA)Activator.CreateInstance(proxyType, serviceProvider, targetApp);
            Assert.NotNull(proxyApp);

            proxyApp.AppId = 3;
            proxyApp.AppName = "proxyApp";

            Assert.Equal(proxyApp.AppId, targetApp.AppId);
            Assert.Equal(proxyApp.AppName, targetApp.AppName);
            Assert.Equal(proxyApp.GetAppType(), targetApp.GetAppType());
            Assert.Equal(proxyApp.RunApp(null), targetApp.RunApp(null));
        }

        [Fact]
        public void InterfaceProxyGenerator_GenerateProxyTypeWithInterceptor_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IAppService));
            var proxyType = interfaceProxyGenerator.GenerateProxyType();

            var targetApp = Substitute.For<IAppService>();
            targetApp.GetAppType().Returns("mockapp");

            var proxyApp = (IAppService)Activator.CreateInstance(proxyType, serviceProvider, targetApp);

            Assert.NotNull(proxyApp);
            Assert.NotEqual(proxyApp.GetAppType(), targetApp.GetAppType());
            Assert.Equal(proxyApp.GetAppType(), "InterceptorApp");
        }

        [Fact]
        public void InterfaceProxyGenerator_GenerateProxyTypeWithParameterNotNull_Test()
        {
            var interfaceProxyGenerator = new InterfaceProxyGenerator(serviceProvider, typeof(IAppService));
            var proxyType = interfaceProxyGenerator.GenerateProxyType();

            var proxyApp = (IAppService)Activator.CreateInstance(proxyType, serviceProvider, new TestAppService(output));

            ExceptionAssert.ThrowsArgumentNull(() => proxyApp.RunApp(null), "args");
        }
    }
}
