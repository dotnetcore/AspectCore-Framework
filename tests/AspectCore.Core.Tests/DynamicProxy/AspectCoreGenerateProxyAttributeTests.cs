using System;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectCoreGenerateProxyAttributeTests
    {
        #region Default Constructor

        [Fact]
        public void DefaultConstructor_SetsServiceTypeToNull()
        {
            var attribute = new AspectCoreGenerateProxyAttribute();
            Assert.Null(attribute.ServiceType);
        }

        [Fact]
        public void DefaultConstructor_SetsImplementationTypeToNull()
        {
            var attribute = new AspectCoreGenerateProxyAttribute();
            Assert.Null(attribute.ImplementationType);
        }

        [Fact]
        public void DefaultConstructor_SetsKindToNull()
        {
            var attribute = new AspectCoreGenerateProxyAttribute();
            Assert.Null(attribute.Kind);
        }

        #endregion

        #region Constructor(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind)

        [Fact]
        public void Constructor_WithNullServiceType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new AspectCoreGenerateProxyAttribute(null, typeof(TestImplementation), SourceGeneratedProxyKind.Interface));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullImplementationType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new AspectCoreGenerateProxyAttribute(typeof(ITestService), null, SourceGeneratedProxyKind.Interface));
            Assert.Equal("implementationType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithValidArguments_StoresAllProperties()
        {
            var attribute = new AspectCoreGenerateProxyAttribute(typeof(ITestService), typeof(TestImplementation), SourceGeneratedProxyKind.Class);
            Assert.Equal(typeof(ITestService), attribute.ServiceType);
            Assert.Equal(typeof(TestImplementation), attribute.ImplementationType);
            Assert.Equal(SourceGeneratedProxyKind.Class, attribute.Kind);
        }

        [Fact]
        public void Constructor_WithInterfaceKind_StoresKind()
        {
            var attribute = new AspectCoreGenerateProxyAttribute(typeof(ITestService), typeof(TestImplementation), SourceGeneratedProxyKind.Interface);
            Assert.Equal(SourceGeneratedProxyKind.Interface, attribute.Kind);
        }

        #endregion

        #region Constructor(Type implementationType)

        [Fact]
        public void Constructor_WithNullImplementationTypeOnly_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new AspectCoreGenerateProxyAttribute((Type)null));
            Assert.Equal("implementationType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithImplementationTypeOnly_StoresImplementationType()
        {
            var attribute = new AspectCoreGenerateProxyAttribute(typeof(TestImplementation));
            Assert.Equal(typeof(TestImplementation), attribute.ImplementationType);
        }

        [Fact]
        public void Constructor_WithImplementationTypeOnly_ServiceTypeIsNull()
        {
            var attribute = new AspectCoreGenerateProxyAttribute(typeof(TestImplementation));
            Assert.Null(attribute.ServiceType);
        }

        [Fact]
        public void Constructor_WithImplementationTypeOnly_KindIsNull()
        {
            var attribute = new AspectCoreGenerateProxyAttribute(typeof(TestImplementation));
            Assert.Null(attribute.Kind);
        }

        #endregion

        #region SourceGeneratedProxyKind Enum

        [Fact]
        public void SourceGeneratedProxyKind_Interface_HasValueZero()
        {
            Assert.Equal(0, (int)SourceGeneratedProxyKind.Interface);
        }

        [Fact]
        public void SourceGeneratedProxyKind_Class_HasValueOne()
        {
            Assert.Equal(1, (int)SourceGeneratedProxyKind.Class);
        }

        #endregion

        #region Test Types

        private interface ITestService
        {
            void Foo();
        }

        private class TestImplementation : ITestService
        {
            public void Foo() { }
        }

        #endregion
    }
}
