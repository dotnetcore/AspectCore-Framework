using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyGeneratorExtendedTests
    {
        private static IProxyGenerator CreateGenerator()
        {
            var builder = new ProxyGeneratorBuilder();
            return builder.Build();
        }

        [Fact]
        public void CreateClassProxy_WithNullServiceType_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateClassProxy(null, typeof(TestService), null));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateClassProxy_WithNullImplementationType_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateClassProxy(typeof(TestService), null, null));
            Assert.Equal("implementationType", ex.ParamName);
        }

        [Fact]
        public void CreateClassProxy_WithNullArgs_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateClassProxy(typeof(TestService), typeof(TestService), null));
            Assert.Equal("args", ex.ParamName);
        }

        [Fact]
        public void CreateInterfaceProxy_WithNullServiceType_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateInterfaceProxy(null));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateInterfaceProxy_WithNullServiceTypeAndInstance_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateInterfaceProxy(null, new TestServiceImpl()));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateInterfaceProxy_WithNullImplementationInstance_CallsSingleArgOverload()
        {
            var generator = CreateGenerator();
            var result = generator.CreateInterfaceProxy(typeof(ITestService), null);
            Assert.NotNull(result);
            Assert.IsAssignableFrom<ITestService>(result);
        }

        [Fact]
        public void CreateClassProxy_WithValidArgs_ReturnsProxy()
        {
            var generator = CreateGenerator();
            var proxy = generator.CreateClassProxy(typeof(TestService), typeof(TestService), new object[0]);
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<TestService>(proxy);
        }

        [Fact]
        public void CreateInterfaceProxy_WithImplementationInstance_ReturnsProxy()
        {
            var generator = CreateGenerator();
            var impl = new TestServiceImpl();
            var proxy = generator.CreateInterfaceProxy(typeof(ITestService), impl);
            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ITestService>(proxy);
        }

        public interface ITestService
        {
            void DoSomething();
        }

        public class TestServiceImpl : ITestService
        {
            public void DoSomething() { }
        }

        public class TestService
        {
            public virtual void DoSomething() { }
        }
    }
}
