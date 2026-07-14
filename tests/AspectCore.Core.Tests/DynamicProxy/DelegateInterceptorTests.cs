using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class DelegateInterceptorTests
    {
        #region Constructor

        [Fact]
        public void Constructor_WithNullDelegate_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DelegateInterceptor(null));
            Assert.Equal("aspectDelegate", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNullDelegateAndOrder_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new DelegateInterceptor(null, 5));
            Assert.Equal("aspectDelegate", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithValidDelegate_CreatesInterceptor()
        {
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            var interceptor = new DelegateInterceptor(aspectDelegate);
            Assert.NotNull(interceptor);
        }

        #endregion

        #region AllowMultiple

        [Fact]
        public void AllowMultiple_IsTrue()
        {
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            var interceptor = new DelegateInterceptor(aspectDelegate);
            Assert.True(interceptor.AllowMultiple);
        }

        #endregion

        #region Order

        [Fact]
        public void Order_DefaultsToZero()
        {
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            var interceptor = new DelegateInterceptor(aspectDelegate);
            Assert.Equal(0, interceptor.Order);
        }

        [Fact]
        public void Order_WithSpecifiedOrder_ReturnsCorrectValue()
        {
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            var interceptor = new DelegateInterceptor(aspectDelegate, 42);
            Assert.Equal(42, interceptor.Order);
        }

        [Fact]
        public void Order_CanBeSet()
        {
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            var interceptor = new DelegateInterceptor(aspectDelegate);
            interceptor.Order = 10;
            Assert.Equal(10, interceptor.Order);
        }

        #endregion

        #region Inherited

        [Fact]
        public void Inherited_DefaultsToFalse()
        {
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            var interceptor = new DelegateInterceptor(aspectDelegate);
            Assert.False(interceptor.Inherited);
        }

        [Fact]
        public void Inherited_CanBeSet()
        {
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            var interceptor = new DelegateInterceptor(aspectDelegate);
            interceptor.Inherited = true;
            Assert.True(interceptor.Inherited);
        }

        #endregion

        #region Invoke

        [Fact]
        public async Task Invoke_CallsTheAspectDelegate()
        {
            bool delegateCalled = false;
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context =>
            {
                delegateCalled = true;
                return next(context);
            };
            var interceptor = new DelegateInterceptor(aspectDelegate);
            var context = new TestAspectContext();

            bool nextCalled = false;
            AspectDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            await interceptor.Invoke(context, next);
            Assert.True(delegateCalled);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_AspectDelegateCanShortCircuit()
        {
            bool nextCalled = false;
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context =>
            {
                // Short circuit - don't call next
                return Task.CompletedTask;
            };
            var interceptor = new DelegateInterceptor(aspectDelegate);
            var context = new TestAspectContext();

            AspectDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            await interceptor.Invoke(context, next);
            Assert.False(nextCalled);
        }

        [Fact]
        public async Task Invoke_PassesContextToDelegate()
        {
            AspectContext receivedContext = null;
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context =>
            {
                receivedContext = context;
                return next(context);
            };
            var interceptor = new DelegateInterceptor(aspectDelegate);
            var context = new TestAspectContext();

            AspectDelegate next = ctx => Task.CompletedTask;

            await interceptor.Invoke(context, next);
            Assert.Same(context, receivedContext);
        }

        [Fact]
        public async Task Invoke_WithContextDelegate_InvokesCorrectly()
        {
            bool delegateCalled = false;
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) =>
            {
                delegateCalled = true;
                return next(context);
            };
            var interceptor = new DelegateInterceptor(next => context => aspectDelegate(context, next));
            var context = new TestAspectContext();

            bool nextCalled = false;
            AspectDelegate nextDelegate = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };

            await interceptor.Invoke(context, nextDelegate);
            Assert.True(delegateCalled);
            Assert.True(nextCalled);
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
