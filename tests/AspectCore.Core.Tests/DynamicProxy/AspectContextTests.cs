using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectContextTests
    {
        #region AdditionalData

        [Fact]
        public void AdditionalData_ReturnsNonNullDictionary()
        {
            var context = new TestAspectContext();
            Assert.NotNull(context.AdditionalData);
        }

        [Fact]
        public void AdditionalData_CanStoreAndRetrieveValues()
        {
            var context = new TestAspectContext();
            context.AdditionalData["key"] = "value";
            Assert.Equal("value", context.AdditionalData["key"]);
        }

        [Fact]
        public void AdditionalData_IsEmptyByDefault()
        {
            var context = new TestAspectContext();
            Assert.Empty(context.AdditionalData);
        }

        #endregion

        #region ReturnValue

        [Fact]
        public void ReturnValue_DefaultsToNull()
        {
            var context = new TestAspectContext();
            Assert.Null(context.ReturnValue);
        }

        [Fact]
        public void ReturnValue_CanBeSetAndRetrieved()
        {
            var context = new TestAspectContext();
            context.ReturnValue = 42;
            Assert.Equal(42, context.ReturnValue);
        }

        [Fact]
        public void ReturnValue_CanBeSetToNull()
        {
            var context = new TestAspectContext();
            context.ReturnValue = "test";
            context.ReturnValue = null;
            Assert.Null(context.ReturnValue);
        }

        #endregion

        #region ServiceProvider

        [Fact]
        public void ServiceProvider_ReturnsConfiguredProvider()
        {
            var provider = new FakeServiceProvider();
            var context = new TestAspectContext(provider);
            Assert.Same(provider, context.ServiceProvider);
        }

        [Fact]
        public void ServiceProvider_DefaultsToNull()
        {
            var context = new TestAspectContext();
            Assert.Null(context.ServiceProvider);
        }

        #endregion

        #region ServiceMethod

        [Fact]
        public void ServiceMethod_ReturnsConfiguredMethod()
        {
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));
            var context = new TestAspectContext(serviceMethod: method);
            Assert.Same(method, context.ServiceMethod);
        }

        #endregion

        #region Parameters

        [Fact]
        public void Parameters_ReturnsConfiguredParameters()
        {
            var parameters = new object[] { 1, "test" };
            var context = new TestAspectContext(parameters: parameters);
            Assert.Same(parameters, context.Parameters);
        }

        [Fact]
        public void Parameters_DefaultsToEmptyArray()
        {
            var context = new TestAspectContext();
            Assert.Empty(context.Parameters);
        }

        #endregion

        #region Break

        [Fact]
        public async Task Break_ReturnsCompletedTask()
        {
            var context = new TestAspectContext();
            var result = context.Break();
            await result;
            Assert.True(result.IsCompleted);
        }

        #endregion

        #region Complete

        [Fact]
        public async Task Complete_ReturnsCompletedTask()
        {
            var context = new TestAspectContext();
            var result = context.Complete();
            await result;
            Assert.True(result.IsCompleted);
        }

        #endregion

        #region Invoke

        [Fact]
        public async Task Invoke_CallsNextDelegate()
        {
            var context = new TestAspectContext();
            bool nextCalled = false;
            AspectDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            await context.Invoke(next);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_PassesContextToNext()
        {
            var context = new TestAspectContext();
            AspectContext receivedContext = null;
            AspectDelegate next = ctx =>
            {
                receivedContext = ctx;
                return Task.CompletedTask;
            };
            await context.Invoke(next);
            Assert.Same(context, receivedContext);
        }

        #endregion

        #region Implementation

        [Fact]
        public void Implementation_DefaultsToNull()
        {
            var context = new TestAspectContext();
            Assert.Null(context.Implementation);
        }

        [Fact]
        public void Implementation_CanBeSet()
        {
            var impl = new TestService();
            var context = new TestAspectContext(implementation: impl);
            Assert.Same(impl, context.Implementation);
        }

        #endregion

        #region Proxy

        [Fact]
        public void Proxy_DefaultsToNull()
        {
            var context = new TestAspectContext();
            Assert.Null(context.Proxy);
        }

        #endregion

        #region IsAbstract

        [Fact]
        public void IsAbstractClass()
        {
            Assert.True(typeof(AspectContext).IsAbstract);
        }

        #endregion

        #region Test Types

        private class TestAspectContext : AspectContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly MethodInfo _serviceMethod;
            private readonly object _implementation;
            private readonly object[] _parameters;

            public TestAspectContext(
                IServiceProvider serviceProvider = null,
                MethodInfo serviceMethod = null,
                object implementation = null,
                object[] parameters = null)
            {
                _serviceProvider = serviceProvider;
                _serviceMethod = serviceMethod;
                _implementation = implementation;
                _parameters = parameters ?? Array.Empty<object>();
            }

            public override IDictionary<string, object> AdditionalData { get; } = new Dictionary<string, object>();

            public override object ReturnValue { get; set; }

            public override IServiceProvider ServiceProvider => _serviceProvider;

            public override MethodInfo ServiceMethod => _serviceMethod;

            public override object Implementation => _implementation;

            public override MethodInfo ImplementationMethod => null;

            public override object[] Parameters => _parameters;

            public override MethodInfo ProxyMethod => null;

            public override MethodInfo PredicateMethod => null;

            public override object Proxy => null;

            public override Task Break() => Task.CompletedTask;

            public override Task Invoke(AspectDelegate next) => next(this);

            public override Task Complete() => Task.CompletedTask;
        }

        private class TestService
        {
            public virtual void Foo() { }

            public virtual void Bar() { }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        #endregion
    }
}
