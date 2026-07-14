using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectInvocationExceptionTests
    {
        #region Constructor(AspectContext, string)

        [Fact]
        public void Constructor_WithContextAndMessage_StoresMessage()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvocationException(context, "test message");
            Assert.Equal("test message", ex.Message);
        }

        [Fact]
        public void Constructor_WithContextAndMessage_StoresContext()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvocationException(context, "test message");
            Assert.Same(context, ex.AspectContext);
        }

        [Fact]
        public void Constructor_WithContextAndMessage_InnerExceptionIsNull()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvocationException(context, "test message");
            Assert.Null(ex.InnerException);
        }

        #endregion

        #region Constructor(AspectContext, Exception)

        [Fact]
        public void Constructor_WithContextAndInnerException_StoresInnerException()
        {
            var context = new TestAspectContext();
            var innerException = new InvalidOperationException("inner error");
            var ex = new AspectInvocationException(context, innerException);
            Assert.Same(innerException, ex.InnerException);
        }

        [Fact]
        public void Constructor_WithContextAndInnerException_StoresContext()
        {
            var context = new TestAspectContext();
            var innerException = new InvalidOperationException("inner error");
            var ex = new AspectInvocationException(context, innerException);
            Assert.Same(context, ex.AspectContext);
        }

        [Fact]
        public void Constructor_WithContextAndInnerException_MessageContainsInnerMessage()
        {
            var context = new TestAspectContext();
            var innerException = new InvalidOperationException("inner error");
            var ex = new AspectInvocationException(context, innerException);
            Assert.Contains("inner error", ex.Message);
        }

        [Fact]
        public void Constructor_WithContextAndNullInnerException_MessageDoesNotThrow()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvocationException(context, (Exception)null);
            Assert.NotNull(ex.Message);
        }

        #endregion

        #region Constructor(AspectContext, string, Exception)

        [Fact]
        public void Constructor_WithContextMessageAndInnerException_StoresAll()
        {
            var context = new TestAspectContext();
            var innerException = new InvalidOperationException("inner error");
            var ex = new AspectInvocationException(context, "outer message", innerException);
            Assert.Equal("outer message", ex.Message);
            Assert.Same(innerException, ex.InnerException);
            Assert.Same(context, ex.AspectContext);
        }

        #endregion

        #region AspectInvalidCastException

        [Fact]
        public void AspectInvalidCastException_IsSubclassOfAspectInvocationException()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvalidCastException(context, "cast error");
            Assert.IsAssignableFrom<AspectInvocationException>(ex);
        }

        [Fact]
        public void AspectInvalidCastException_StoresMessage()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvalidCastException(context, "cast error");
            Assert.Equal("cast error", ex.Message);
        }

        [Fact]
        public void AspectInvalidCastException_StoresContext()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvalidCastException(context, "cast error");
            Assert.Same(context, ex.AspectContext);
        }

        #endregion

        #region AspectInvalidOperationException

        [Fact]
        public void AspectInvalidOperationException_IsSubclassOfAspectInvocationException()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvalidOperationException(context, "operation error");
            Assert.IsAssignableFrom<AspectInvocationException>(ex);
        }

        [Fact]
        public void AspectInvalidOperationException_StoresMessage()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvalidOperationException(context, "operation error");
            Assert.Equal("operation error", ex.Message);
        }

        [Fact]
        public void AspectInvalidOperationException_StoresContext()
        {
            var context = new TestAspectContext();
            var ex = new AspectInvalidOperationException(context, "operation error");
            Assert.Same(context, ex.AspectContext);
        }

        #endregion

        #region Test Types

        private class TestAspectContext : AspectContext
        {
            public override System.Collections.Generic.IDictionary<string, object> AdditionalData => new System.Collections.Generic.Dictionary<string, object>();

            public override object ReturnValue { get; set; }

            public override IServiceProvider ServiceProvider => null;

            public override System.Reflection.MethodInfo ServiceMethod => null;

            public override object Implementation => null;

            public override System.Reflection.MethodInfo ImplementationMethod => null;

            public override object[] Parameters => Array.Empty<object>();

            public override System.Reflection.MethodInfo ProxyMethod => null;

            public override System.Reflection.MethodInfo PredicateMethod => null;

            public override object Proxy => null;

            public override Task Break() => Task.CompletedTask;

            public override Task Invoke(AspectDelegate next) => next(this);

            public override Task Complete() => Task.CompletedTask;
        }

        #endregion
    }
}
