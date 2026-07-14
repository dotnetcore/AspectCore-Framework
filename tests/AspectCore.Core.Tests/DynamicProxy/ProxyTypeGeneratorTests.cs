using System;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyTypeGeneratorTests
    {
        private static IAspectValidatorBuilder CreateValidatorBuilder()
        {
            return new AspectValidatorBuilder(new AspectConfiguration());
        }

        private static ProxyTypeGenerator CreateGenerator()
        {
            return new ProxyTypeGenerator(CreateValidatorBuilder());
        }

        #region Constructor

        [Fact]
        public void Constructor_NullAspectValidatorBuilder_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ProxyTypeGenerator(null));
            Assert.Equal("aspectValidatorBuilder", ex.ParamName);
        }

        [Fact]
        public void Constructor_ValidArguments_CreatesInstance()
        {
            var generator = CreateGenerator();
            Assert.NotNull(generator);
        }

        #endregion

        #region CreateInterfaceProxyType(Type)

        [Fact]
        public void CreateInterfaceProxyType_NullServiceType_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() => generator.CreateInterfaceProxyType(null));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateInterfaceProxyType_NonInterface_ThrowsArgumentException()
        {
            var generator = CreateGenerator();
            Assert.Throws<ArgumentException>(() => generator.CreateInterfaceProxyType(typeof(PtgTestClass)));
        }

        [Fact]
        public void CreateInterfaceProxyType_ValidInterface_ReturnsProxyType()
        {
            var generator = CreateGenerator();
            var proxyType = generator.CreateInterfaceProxyType(typeof(IPtgTestService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(IPtgTestService).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(IPtgTestService), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_SameInputTwice_ReturnsSameType()
        {
            var generator = CreateGenerator();
            var first = generator.CreateInterfaceProxyType(typeof(IPtgTestService));
            var second = generator.CreateInterfaceProxyType(typeof(IPtgTestService));
            Assert.Same(first, second);
        }

        #endregion

        #region CreateInterfaceProxyType(Type, Type)

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_NullServiceType_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateInterfaceProxyType(null, typeof(PtgTestService)));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_NonInterface_ThrowsArgumentException()
        {
            var generator = CreateGenerator();
            Assert.Throws<ArgumentException>(() =>
                generator.CreateInterfaceProxyType(typeof(PtgTestClass), typeof(PtgTestService)));
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_ValidArguments_ReturnsProxyType()
        {
            var generator = CreateGenerator();
            var proxyType = generator.CreateInterfaceProxyType(typeof(IPtgTestService), typeof(PtgTestService));
            Assert.NotNull(proxyType);
            Assert.True(typeof(IPtgTestService).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(IPtgTestService), proxyType);
        }

        [Fact]
        public void CreateInterfaceProxyType_WithImpl_SameInputTwice_ReturnsSameType()
        {
            var generator = CreateGenerator();
            var first = generator.CreateInterfaceProxyType(typeof(IPtgTestService), typeof(PtgTestService));
            var second = generator.CreateInterfaceProxyType(typeof(IPtgTestService), typeof(PtgTestService));
            Assert.Same(first, second);
        }

        #endregion

        #region CreateClassProxyType(Type, Type)

        [Fact]
        public void CreateClassProxyType_NullServiceType_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxyType(null, typeof(PtgTestClass)));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void CreateClassProxyType_NullImplementationType_ThrowsArgumentNullException()
        {
            var generator = CreateGenerator();
            // ProxyTypeGenerator does not check implementationType directly;
            // it passes it to GetInterfaces() which calls GetTypeInfo(),
            // throwing ArgumentNullException with param name "type".
            var ex = Assert.Throws<ArgumentNullException>(() =>
                generator.CreateClassProxyType(typeof(PtgTestClass), null));
            Assert.Equal("type", ex.ParamName);
        }

        [Fact]
        public void CreateClassProxyType_InterfaceServiceType_ThrowsArgumentException()
        {
            var generator = CreateGenerator();
            Assert.Throws<ArgumentException>(() =>
                generator.CreateClassProxyType(typeof(IPtgTestService), typeof(PtgTestService)));
        }

        [Fact]
        public void CreateClassProxyType_ValidClass_ReturnsProxyType()
        {
            var generator = CreateGenerator();
            var proxyType = generator.CreateClassProxyType(typeof(PtgTestClass), typeof(PtgTestClass));
            Assert.NotNull(proxyType);
            Assert.True(typeof(PtgTestClass).IsAssignableFrom(proxyType));
            Assert.NotEqual(typeof(PtgTestClass), proxyType);
        }

        [Fact]
        public void CreateClassProxyType_SameInputTwice_ReturnsSameType()
        {
            var generator = CreateGenerator();
            var first = generator.CreateClassProxyType(typeof(PtgTestClass), typeof(PtgTestClass));
            var second = generator.CreateClassProxyType(typeof(PtgTestClass), typeof(PtgTestClass));
            Assert.Same(first, second);
        }

        #endregion

        #region Test Types

        public interface IPtgTestService
        {
            int Add(int a, int b);
            string Name { get; }
        }

        public class PtgTestService : IPtgTestService
        {
            public int Add(int a, int b) => a + b;
            public string Name => "TestService";
        }

        public class PtgTestClass
        {
            public virtual int Add(int a, int b) => a + b;
            public virtual string Name => "TestClass";
        }

        #endregion
    }
}
