using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class ServiceInterceptorFactoryTests
    {
        #region Constructor

        [Fact]
        public void Constructor_WithValidInterceptorType_CreatesFactory()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            Assert.NotNull(factory);
        }

        [Fact]
        public void Constructor_WithValidInterceptorType_AndPredicates_StoresPredicates()
        {
            AspectPredicate predicate = method => true;

            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor), predicate);

            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void Constructor_WithValidInterceptorType_NoPredicates_HasEmptyPredicates()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            Assert.Empty(factory.Predicates);
        }

        [Fact]
        public void Constructor_WithValidInterceptorAttributeType_CreatesFactory()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptorAttribute));

            Assert.NotNull(factory);
        }

        [Fact]
        public void Constructor_WithNullType_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ServiceInterceptorFactory(null));

            Assert.Equal("interceptorType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNonInterceptorType_ThrowsArgumentException()
        {
            var ex = Assert.Throws<ArgumentException>(() => new ServiceInterceptorFactory(typeof(NotAnInterceptor)));

            Assert.Equal("interceptorType", ex.ParamName);
        }

        [Fact]
        public void Constructor_WithNonInterceptorType_ExceptionMessageContainsTypeName()
        {
            var ex = Assert.Throws<ArgumentException>(() => new ServiceInterceptorFactory(typeof(NotAnInterceptor)));

            Assert.Contains(typeof(NotAnInterceptor).ToString(), ex.Message);
        }

        #endregion

        #region CreateInstance

        [Fact]
        public void CreateInstance_ReturnsServiceInterceptorAttribute()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            var interceptor = factory.CreateInstance(null);

            Assert.IsType<ServiceInterceptorAttribute>(interceptor);
        }

        [Fact]
        public void CreateInstance_ReturnsNonNullInterceptor()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            var interceptor = factory.CreateInstance(null);

            Assert.NotNull(interceptor);
        }

        [Fact]
        public void CreateInstance_ReturnsInterceptorImplementingIInterceptor()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            var interceptor = factory.CreateInstance(null);

            Assert.IsAssignableFrom<IInterceptor>(interceptor);
        }

        [Fact]
        public void CreateInstance_WithNullServiceProvider_ReturnsServiceInterceptorAttribute()
        {
            IServiceProvider serviceProvider = null;
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            var interceptor = factory.CreateInstance(serviceProvider);

            Assert.IsType<ServiceInterceptorAttribute>(interceptor);
        }

        [Fact]
        public void CreateInstance_WithNonNullServiceProvider_ReturnsServiceInterceptorAttribute()
        {
            IServiceProvider serviceProvider = new FakeServiceProvider();
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            var interceptor = factory.CreateInstance(serviceProvider);

            Assert.IsType<ServiceInterceptorAttribute>(interceptor);
        }

        [Fact]
        public void CreateInstance_MultipleCalls_ReturnsEqualInstances()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            var first = factory.CreateInstance(null);
            var second = factory.CreateInstance(null);

            Assert.Equal(first, second);
        }

        [Fact]
        public void CreateInstance_ForDifferentInterceptorTypes_ReturnsNotEqualInstances()
        {
            var factory1 = new ServiceInterceptorFactory(typeof(TestInterceptor));
            var factory2 = new ServiceInterceptorFactory(typeof(TestInterceptorAttribute));

            var first = factory1.CreateInstance(null);
            var second = factory2.CreateInstance(null);

            Assert.NotEqual(first, second);
        }

        [Fact]
        public void CreateInstance_ReturnedAttribute_AllowMultipleIsTrue()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            var interceptor = factory.CreateInstance(null);

            Assert.True(interceptor.AllowMultiple);
        }

        #endregion

        #region CanCreated / Predicate Evaluation

        [Fact]
        public void CanCreated_WithNoPredicates_ReturnsTrueForAnyMethod()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithMatchingPredicate_ReturnsTrue()
        {
            AspectPredicate predicate = method => method.Name == "Foo";
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor), predicate);
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithNonMatchingPredicate_ReturnsFalse()
        {
            AspectPredicate predicate = method => method.Name == "Foo";
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor), predicate);
            var method = typeof(TestService).GetMethod(nameof(TestService.Bar));

            Assert.False(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithMultiplePredicates_ReturnsTrueWhenAnyMatches()
        {
            AspectPredicate predicate1 = method => method.Name == "NonExistent";
            AspectPredicate predicate2 = method => method.Name == "Foo";
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor), predicate1, predicate2);
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithMultiplePredicates_ReturnsFalseWhenNoneMatch()
        {
            AspectPredicate predicate1 = method => method.Name == "NonExistent";
            AspectPredicate predicate2 = method => method.Name == "AnotherNonExistent";
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor), predicate1, predicate2);
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.False(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithPredicateMatchingByDeclaringType_ReturnsTrue()
        {
            AspectPredicate predicate = method => method.DeclaringType == typeof(TestService);
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor), predicate);
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));

            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void Predicates_AreStoredInOrder()
        {
            AspectPredicate predicate1 = method => true;
            AspectPredicate predicate2 = method => false;
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor), predicate1, predicate2);

            Assert.Equal(2, factory.Predicates.Length);
            Assert.Same(predicate1, factory.Predicates[0]);
            Assert.Same(predicate2, factory.Predicates[1]);
        }

        [Fact]
        public void CanCreated_WithNullMethod_DoesNotThrowWhenNoPredicates()
        {
            var factory = new ServiceInterceptorFactory(typeof(TestInterceptor));

            Assert.True(factory.CanCreated(null));
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

        private class TestService
        {
            public virtual void Foo() { }

            public virtual void Bar() { }
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType)
            {
                return null;
            }
        }

        #endregion
    }
}
