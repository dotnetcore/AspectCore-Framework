using System;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class TypeInterceptorFactoryTests
    {
        [Fact]
        public void Constructor_WithNullInterceptorType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new TypeInterceptorFactory(null, null));
            Assert.Equal("interceptorType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNonInterceptorType_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new TypeInterceptorFactory(typeof(string), null));
            Assert.Equal("interceptorType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithValidInterceptorType_DoesNotThrow()
        {
            var factory = new TypeInterceptorFactory(typeof(TestInterceptor), null);
            Assert.NotNull(factory);
        }

        [Fact]
        public void CreateInstance_ReturnsInterceptorInstance()
        {
            var factory = new TypeInterceptorFactory(typeof(TestInterceptor), null);
            var result = factory.CreateInstance(null);
            Assert.NotNull(result);
            Assert.IsType<TestInterceptor>(result);
        }

        [Fact]
        public void CreateInstance_WithArgs_PassesArgsToConstructor()
        {
            var factory = new TypeInterceptorFactory(typeof(TestInterceptorWithArgs), new object[] { "test" });
            var result = factory.CreateInstance(null);
            Assert.NotNull(result);
            var interceptor = Assert.IsType<TestInterceptorWithArgs>(result);
            Assert.Equal("test", interceptor.Value);
        }

        [Fact]
        public void CanCreated_WithNoPredicates_ReturnsTrue()
        {
            var factory = new TypeInterceptorFactory(typeof(TestInterceptor), null);
            var method = typeof(TestService).GetMethod(nameof(TestService.DoSomething));
            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithMatchingPredicate_ReturnsTrue()
        {
            AspectPredicate predicate = m => m.Name == "DoSomething";
            var factory = new TypeInterceptorFactory(typeof(TestInterceptor), null, predicate);
            var method = typeof(TestService).GetMethod(nameof(TestService.DoSomething));
            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithNonMatchingPredicate_ReturnsFalse()
        {
            AspectPredicate predicate = m => m.Name == "OtherMethod";
            var factory = new TypeInterceptorFactory(typeof(TestInterceptor), null, predicate);
            var method = typeof(TestService).GetMethod(nameof(TestService.DoSomething));
            Assert.False(factory.CanCreated(method));
        }

        public class TestInterceptor : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next) => next(context);
        }

        public class TestInterceptorWithArgs : AbstractInterceptorAttribute
        {
            public string Value { get; }
            public TestInterceptorWithArgs(string value) => Value = value;
            public override Task Invoke(AspectContext context, AspectDelegate next) => next(context);
        }

        public class TestService
        {
            public virtual void DoSomething() { }
        }
    }
}
