using System;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AbstractInterceptorTests
    {
        #region Default Values

        [Fact]
        public void AllowMultiple_DefaultsToFalse()
        {
            var interceptor = new TestInterceptor();
            Assert.False(interceptor.AllowMultiple);
        }

        [Fact]
        public void Order_DefaultsToZero()
        {
            var interceptor = new TestInterceptor();
            Assert.Equal(0, interceptor.Order);
        }

        [Fact]
        public void Inherited_DefaultsToFalse()
        {
            var interceptor = new TestInterceptor();
            Assert.False(interceptor.Inherited);
        }

        #endregion

        #region Order

        [Fact]
        public void Order_CanBeSet()
        {
            var interceptor = new TestInterceptor();
            interceptor.Order = 5;
            Assert.Equal(5, interceptor.Order);
        }

        [Fact]
        public void Order_CanBeNegative()
        {
            var interceptor = new TestInterceptor();
            interceptor.Order = -1;
            Assert.Equal(-1, interceptor.Order);
        }

        #endregion

        #region Inherited

        [Fact]
        public void Inherited_CanBeSetToTrue()
        {
            var interceptor = new TestInterceptor();
            interceptor.Inherited = true;
            Assert.True(interceptor.Inherited);
        }

        #endregion

        #region Invoke

        [Fact]
        public async Task Invoke_CallsNextDelegate()
        {
            var interceptor = new TestInterceptor();
            bool nextCalled = false;
            AspectDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            await interceptor.Invoke(null, next);
            Assert.True(nextCalled);
        }

        [Fact]
        public async Task Invoke_ReturnsTaskFromNext()
        {
            var interceptor = new TestInterceptor();
            AspectDelegate next = ctx => Task.CompletedTask;
            var result = interceptor.Invoke(null, next);
            await result;
            Assert.True(result.IsCompleted);
        }

        #endregion

        #region IInterceptor Interface

        [Fact]
        public void ImplementsIInterceptor()
        {
            var interceptor = new TestInterceptor();
            Assert.IsAssignableFrom<IInterceptor>(interceptor);
        }

        #endregion

        #region AllowMultiple Override

        [Fact]
        public void AllowMultiple_CanBeOverridden()
        {
            var interceptor = new TestInterceptorAllowMultiple();
            Assert.True(interceptor.AllowMultiple);
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

        private class TestInterceptorAllowMultiple : AbstractInterceptor
        {
            public override bool AllowMultiple => true;

            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        #endregion
    }

    public class AbstractInterceptorAttributeTests
    {
        #region Default Values

        [Fact]
        public void AllowMultiple_DefaultsToFalse()
        {
            var attribute = new TestInterceptorAttribute();
            Assert.False(attribute.AllowMultiple);
        }

        [Fact]
        public void Order_DefaultsToZero()
        {
            var attribute = new TestInterceptorAttribute();
            Assert.Equal(0, attribute.Order);
        }

        [Fact]
        public void Inherited_DefaultsToFalse()
        {
            var attribute = new TestInterceptorAttribute();
            Assert.False(attribute.Inherited);
        }

        #endregion

        #region Order

        [Fact]
        public void Order_CanBeSet()
        {
            var attribute = new TestInterceptorAttribute();
            attribute.Order = 10;
            Assert.Equal(10, attribute.Order);
        }

        #endregion

        #region Inherited

        [Fact]
        public void Inherited_CanBeSetToTrue()
        {
            var attribute = new TestInterceptorAttribute();
            attribute.Inherited = true;
            Assert.True(attribute.Inherited);
        }

        #endregion

        #region Invoke

        [Fact]
        public async Task Invoke_CallsNextDelegate()
        {
            var attribute = new TestInterceptorAttribute();
            bool nextCalled = false;
            AspectDelegate next = ctx =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            };
            await attribute.Invoke(null, next);
            Assert.True(nextCalled);
        }

        #endregion

        #region IInterceptor Interface

        [Fact]
        public void ImplementsIInterceptor()
        {
            var attribute = new TestInterceptorAttribute();
            Assert.IsAssignableFrom<IInterceptor>(attribute);
        }

        #endregion

        #region IsAttribute

        [Fact]
        public void IsAttribute()
        {
            var attribute = new TestInterceptorAttribute();
            Assert.IsAssignableFrom<Attribute>(attribute);
        }

        #endregion

        #region AllowMultiple Override

        [Fact]
        public void AllowMultiple_CanBeOverridden()
        {
            var attribute = new TestInterceptorAttributeAllowMultiple();
            Assert.True(attribute.AllowMultiple);
        }

        #endregion

        #region Test Types

        private class TestInterceptorAttribute : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        private class TestInterceptorAttributeAllowMultiple : AbstractInterceptorAttribute
        {
            public override bool AllowMultiple => true;

            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        #endregion
    }
}
