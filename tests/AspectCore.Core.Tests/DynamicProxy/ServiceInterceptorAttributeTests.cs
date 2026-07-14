using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class ServiceInterceptorAttributeTests
    {
        #region Constructor

        [Fact]
        public void Constructor_WithNullType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ServiceInterceptorAttribute(null));
            Assert.Equal("interceptorType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNonInterceptorType_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new ServiceInterceptorAttribute(typeof(NotAnInterceptor)));
            Assert.Equal("interceptorType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNonInterceptorType_ExceptionMessageContainsTypeName()
        {
            var ex = Assert.Throws<ArgumentException>(() => new ServiceInterceptorAttribute(typeof(NotAnInterceptor)));
            Assert.Contains(typeof(NotAnInterceptor).ToString(), ex.Message);
        }

        [Fact]
        public void Constructor_WithValidInterceptorType_CreatesAttribute()
        {
            var attribute = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.NotNull(attribute);
        }

        [Fact]
        public void Constructor_WithValidInterceptorAttributeType_CreatesAttribute()
        {
            var attribute = new ServiceInterceptorAttribute(typeof(TestInterceptorAttribute));
            Assert.NotNull(attribute);
        }

        #endregion

        #region AllowMultiple

        [Fact]
        public void AllowMultiple_IsTrue()
        {
            var attribute = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.True(attribute.AllowMultiple);
        }

        #endregion

        #region Equals (IEquatable<ServiceInterceptorAttribute>)

        [Fact]
        public void Equals_SameInterceptorType_ReturnsTrue()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            var attr2 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.True(attr1.Equals(attr2));
        }

        [Fact]
        public void Equals_DifferentInterceptorType_ReturnsFalse()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            var attr2 = new ServiceInterceptorAttribute(typeof(TestInterceptorAttribute));
            Assert.False(attr1.Equals(attr2));
        }

        [Fact]
        public void Equals_NullOther_ReturnsFalse()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.False(attr1.Equals(null));
        }

        #endregion

        #region Equals (object)

        [Fact]
        public void Equals_Object_SameInterceptorType_ReturnsTrue()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            object attr2 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.True(attr1.Equals(attr2));
        }

        [Fact]
        public void Equals_Object_DifferentType_ReturnsFalse()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.False(attr1.Equals("not an attribute"));
        }

        [Fact]
        public void Equals_Object_NullObject_ReturnsFalse()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.False(attr1.Equals((object)null));
        }

        #endregion

        #region GetHashCode

        [Fact]
        public void GetHashCode_SameInterceptorType_ReturnsSameHashCode()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            var attr2 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            Assert.Equal(attr1.GetHashCode(), attr2.GetHashCode());
        }

        [Fact]
        public void GetHashCode_DifferentInterceptorType_ReturnsDifferentHashCode()
        {
            var attr1 = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            var attr2 = new ServiceInterceptorAttribute(typeof(TestInterceptorAttribute));
            Assert.NotEqual(attr1.GetHashCode(), attr2.GetHashCode());
        }

        #endregion

        #region Invoke

        [Fact]
        public async Task Invoke_WithResolvedInterceptor_CallsInterceptorInvoke()
        {
            var serviceProvider = new FakeServiceProvider(new TestInterceptor());
            var attribute = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            var context = new TestAspectContext(serviceProvider);

            bool nextCalled = false;
            AspectDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            await attribute.Invoke(context, next);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_WithUnresolvedInterceptor_ThrowsInvalidOperationException()
        {
            var serviceProvider = new FakeServiceProvider(null);
            var attribute = new ServiceInterceptorAttribute(typeof(TestInterceptor));
            var context = new TestAspectContext(serviceProvider);

            AspectDelegate next = ctx => Task.CompletedTask;

            await Assert.ThrowsAsync<InvalidOperationException>(() => attribute.Invoke(context, next));
        }

        #endregion

        #region Test Types

        private class TestInterceptor : AbstractInterceptor
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        private class TestInterceptorAttribute : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        private class NotAnInterceptor
        {
            public void Foo() { }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            private readonly object _service;

            public FakeServiceProvider(object service)
            {
                _service = service;
            }

            public object GetService(Type serviceType)
            {
                return _service;
            }
        }

        private class TestAspectContext : AspectContext
        {
            private readonly IServiceProvider _serviceProvider;

            public TestAspectContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public override System.Collections.Generic.IDictionary<string, object> AdditionalData => new System.Collections.Generic.Dictionary<string, object>();

            public override object ReturnValue { get; set; }

            public override IServiceProvider ServiceProvider => _serviceProvider;

            public override MethodInfo ServiceMethod => typeof(TestService).GetMethod(nameof(TestService.Foo));

            public override object Implementation => null;

            public override MethodInfo ImplementationMethod => null;

            public override object[] Parameters => Array.Empty<object>();

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
        }

        #endregion
    }
}
