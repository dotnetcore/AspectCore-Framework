using System;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.ProxyBuilder;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyTypeCompilerTests
    {
        private static ProxyTypeCompiler CreateCompiler()
        {
            return new ProxyTypeCompiler();
        }

        [Fact]
        public void CreateInterfaceProxy_WithNonInterfaceType_ThrowsInvalidOperationException()
        {
            var compiler = CreateCompiler();
            Assert.Throws<InvalidOperationException>(() =>
                compiler.CreateInterfaceProxy(typeof(string), new Type[0], null));
        }

        [Fact]
        public void CreateInterfaceProxy_WithPrivateInterface_ThrowsInvalidOperationException()
        {
            var compiler = CreateCompiler();
            Assert.Throws<InvalidOperationException>(() =>
                compiler.CreateInterfaceProxy(typeof(IPrivateInterface), new Type[0], null));
        }

        [Fact]
        public void CreateInterfaceProxy_WithImplType_WithNonInterfaceType_ThrowsInvalidOperationException()
        {
            var compiler = CreateCompiler();
            Assert.Throws<InvalidOperationException>(() =>
                compiler.CreateInterfaceProxy(typeof(string), typeof(string), new Type[0], null));
        }

        [Fact]
        public void CreateClassProxy_WithNonClassType_ThrowsInvalidOperationException()
        {
            var compiler = CreateCompiler();
            Assert.Throws<InvalidOperationException>(() =>
                compiler.CreateClassProxy(typeof(int), typeof(int), new Type[0], null));
        }

        [Fact]
        public void CreateClassProxy_WithInterfaceType_ThrowsInvalidOperationException()
        {
            var compiler = CreateCompiler();
            Assert.Throws<InvalidOperationException>(() =>
                compiler.CreateClassProxy(typeof(IDisposable), typeof(IDisposable), new Type[0], null));
        }

        [Fact]
        public void CreateClassProxy_WithSealedImplType_ThrowsInvalidOperationException()
        {
            var compiler = CreateCompiler();
            Assert.Throws<InvalidOperationException>(() =>
                compiler.CreateClassProxy(typeof(SealedClass), typeof(SealedClass), new Type[0], null));
        }

        private interface IPrivateInterface { }

        public interface ITestService
        {
            void DoSomething();
        }

        public class TestClass
        {
            public virtual void DoSomething() { }
        }

        public sealed class SealedClass { }
    }
}
