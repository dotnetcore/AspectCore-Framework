using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using AspectCore.DynamicProxy.ProxyBuilder;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ProxyTypeCompilerValidTests
    {
        private static ProxyTypeCompiler CreateCompiler()
        {
            return new ProxyTypeCompiler();
        }

        private static IAspectValidator CreateValidator()
        {
            return new SimpleAspectValidator();
        }

        [Fact]
        public void CreateInterfaceProxy_WithValidInterface_BuildsProxyType()
        {
            var compiler = CreateCompiler();
            var result = compiler.CreateInterfaceProxy(typeof(ITestService), new Type[0], CreateValidator());
            Assert.NotNull(result);
        }

        [Fact]
        public void CreateInterfaceProxy_WithAdditionalInterfaces_BuildsProxyType()
        {
            var compiler = CreateCompiler();
            var result = compiler.CreateInterfaceProxy(typeof(ITestService), new Type[] { typeof(IDisposable) }, CreateValidator());
            Assert.NotNull(result);
        }

        [Fact]
        public void CreateInterfaceProxy_WithImplType_BuildsProxyType()
        {
            var compiler = CreateCompiler();
            var result = compiler.CreateInterfaceProxy(typeof(ITestService), typeof(TestServiceImpl), new Type[0], CreateValidator());
            Assert.NotNull(result);
        }

        [Fact]
        public void CreateClassProxy_WithValidClass_BuildsProxyType()
        {
            var compiler = CreateCompiler();
            var result = compiler.CreateClassProxy(typeof(TestClass), typeof(TestClass), new Type[0], CreateValidator());
            Assert.NotNull(result);
        }

        [Fact]
        public void CreateClassProxy_WithAdditionalInterfaces_BuildsProxyType()
        {
            var compiler = CreateCompiler();
            var result = compiler.CreateClassProxy(typeof(TestClassWithInterface), typeof(TestClassWithInterface), new Type[] { typeof(ITestService) }, CreateValidator());
            Assert.NotNull(result);
        }

        public interface ITestService
        {
            void DoSomething();
        }

        public class TestServiceImpl : ITestService
        {
            public void DoSomething() { }
        }

        public class TestClass
        {
            public virtual void DoSomething() { }
        }

        public class TestClassWithInterface : ITestService
        {
            public void DoSomething() { }
        }

        private class SimpleAspectValidator : IAspectValidator
        {
            public bool Validate(MethodInfo method, bool isStrictValidation) => false;
        }
    }
}
