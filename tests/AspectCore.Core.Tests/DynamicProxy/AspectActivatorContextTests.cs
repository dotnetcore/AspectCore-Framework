using System;
using System.Reflection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectActivatorContextTests
    {
        private static MethodInfo GetMethod(string name) => typeof(TestService).GetMethod(name);

        #region Constructor

        [Fact]
        public void Constructor_StoresAllProperties()
        {
            var serviceMethod = GetMethod(nameof(TestService.Foo));
            var targetMethod = GetMethod(nameof(TestService.Foo));
            var proxyMethod = GetMethod(nameof(TestService.Foo));
            var predicateMethod = GetMethod(nameof(TestService.Foo));
            var targetInstance = new TestService();
            var proxyInstance = new TestService();
            var parameters = new object[] { 42, "test" };

            var context = new AspectActivatorContext(serviceMethod, targetMethod, proxyMethod, predicateMethod, targetInstance, proxyInstance, parameters);

            Assert.Same(serviceMethod, context.ServiceMethod);
            Assert.Same(targetMethod, context.TargetMethod);
            Assert.Same(proxyMethod, context.ProxyMethod);
            Assert.Same(predicateMethod, context.PredicateMethod);
            Assert.Same(targetInstance, context.TargetInstance);
            Assert.Same(proxyInstance, context.ProxyInstance);
            Assert.Same(parameters, context.Parameters);
        }

        [Fact]
        public void Constructor_AllowsNullMethodInfo()
        {
            var context = new AspectActivatorContext(null, null, null, null, null, null, null);

            Assert.Null(context.ServiceMethod);
            Assert.Null(context.TargetMethod);
            Assert.Null(context.ProxyMethod);
            Assert.Null(context.PredicateMethod);
            Assert.Null(context.TargetInstance);
            Assert.Null(context.ProxyInstance);
            Assert.Null(context.Parameters);
        }

        [Fact]
        public void Constructor_AllowsNullParameters()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context = new AspectActivatorContext(method, method, method, method, null, null, null);

            Assert.Null(context.Parameters);
        }

        #endregion

        #region Properties

        [Fact]
        public void ServiceMethod_ReturnsValueFromConstructor()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context = new AspectActivatorContext(method, null, null, null, null, null, null);
            Assert.Same(method, context.ServiceMethod);
        }

        [Fact]
        public void TargetMethod_ReturnsValueFromConstructor()
        {
            var method = GetMethod(nameof(TestService.Bar));
            var context = new AspectActivatorContext(null, method, null, null, null, null, null);
            Assert.Same(method, context.TargetMethod);
        }

        [Fact]
        public void ProxyMethod_ReturnsValueFromConstructor()
        {
            var method = GetMethod(nameof(TestService.Foo));
            var context = new AspectActivatorContext(null, null, method, null, null, null, null);
            Assert.Same(method, context.ProxyMethod);
        }

        [Fact]
        public void PredicateMethod_ReturnsValueFromConstructor()
        {
            var method = GetMethod(nameof(TestService.Bar));
            var context = new AspectActivatorContext(null, null, null, method, null, null, null);
            Assert.Same(method, context.PredicateMethod);
        }

        [Fact]
        public void TargetInstance_ReturnsValueFromConstructor()
        {
            var instance = new TestService();
            var context = new AspectActivatorContext(null, null, null, null, instance, null, null);
            Assert.Same(instance, context.TargetInstance);
        }

        [Fact]
        public void ProxyInstance_ReturnsValueFromConstructor()
        {
            var instance = new TestService();
            var context = new AspectActivatorContext(null, null, null, null, null, instance, null);
            Assert.Same(instance, context.ProxyInstance);
        }

        [Fact]
        public void Parameters_ReturnsValueFromConstructor()
        {
            var parameters = new object[] { 1, "two", 3.0 };
            var context = new AspectActivatorContext(null, null, null, null, null, null, parameters);
            Assert.Same(parameters, context.Parameters);
        }

        [Fact]
        public void Parameters_CanBeEnumerated()
        {
            var parameters = new object[] { 42, "test" };
            var context = new AspectActivatorContext(null, null, null, null, null, null, parameters);
            Assert.Equal(2, context.Parameters.Length);
            Assert.Equal(42, context.Parameters[0]);
            Assert.Equal("test", context.Parameters[1]);
        }

        #endregion

        #region IsStruct

        [Fact]
        public void IsStruct()
        {
            Assert.True(typeof(AspectActivatorContext).IsValueType);
        }

        #endregion

        #region Test Types

        private class TestService
        {
            public virtual void Foo() { }

            public virtual int Bar(int value) => value;
        }

        #endregion
    }
}
