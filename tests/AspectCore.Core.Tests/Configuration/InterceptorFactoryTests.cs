using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class InterceptorFactoryTests
    {
        private static MethodInfo GetMethod(string name) => typeof(TestService).GetMethod(name);

        #region Constructor

        [Fact]
        public void Constructor_WithNullPredicates_HasEmptyPredicates()
        {
            var factory = new TestFactory(null);
            Assert.Empty(factory.Predicates);
        }

        [Fact]
        public void Constructor_WithEmptyPredicates_HasEmptyPredicates()
        {
            var factory = new TestFactory();
            Assert.Empty(factory.Predicates);
        }

        [Fact]
        public void Constructor_WithPredicates_StoresPredicates()
        {
            AspectPredicate predicate1 = m => true;
            AspectPredicate predicate2 = m => false;
            var factory = new TestFactory(predicate1, predicate2);
            Assert.Equal(2, factory.Predicates.Length);
            Assert.Same(predicate1, factory.Predicates[0]);
            Assert.Same(predicate2, factory.Predicates[1]);
        }

        #endregion

        #region CanCreated

        [Fact]
        public void CanCreated_WithNoPredicates_ReturnsTrueForAnyMethod()
        {
            var factory = new TestFactory();
            var method = GetMethod(nameof(TestService.Foo));
            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithNoPredicates_ReturnsTrueForNullMethod()
        {
            var factory = new TestFactory();
            Assert.True(factory.CanCreated(null));
        }

        [Fact]
        public void CanCreated_WithMatchingPredicate_ReturnsTrue()
        {
            AspectPredicate predicate = m => m.Name == "Foo";
            var factory = new TestFactory(predicate);
            var method = GetMethod(nameof(TestService.Foo));
            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithNonMatchingPredicate_ReturnsFalse()
        {
            AspectPredicate predicate = m => m.Name == "Foo";
            var factory = new TestFactory(predicate);
            var method = GetMethod(nameof(TestService.Bar));
            Assert.False(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithMultiplePredicates_ReturnsTrueWhenAnyMatches()
        {
            AspectPredicate predicate1 = m => m.Name == "NonExistent";
            AspectPredicate predicate2 = m => m.Name == "Foo";
            var factory = new TestFactory(predicate1, predicate2);
            var method = GetMethod(nameof(TestService.Foo));
            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void CanCreated_WithMultiplePredicates_ReturnsFalseWhenNoneMatch()
        {
            AspectPredicate predicate1 = m => m.Name == "NonExistent1";
            AspectPredicate predicate2 = m => m.Name == "NonExistent2";
            var factory = new TestFactory(predicate1, predicate2);
            var method = GetMethod(nameof(TestService.Foo));
            Assert.False(factory.CanCreated(method));
        }

        #endregion

        #region CreateInstance

        [Fact]
        public void CreateInstance_ReturnsNonNullInterceptor()
        {
            var factory = new TestFactory();
            var interceptor = factory.CreateInstance(null);
            Assert.NotNull(interceptor);
        }

        [Fact]
        public void CreateInstance_ReturnsCorrectType()
        {
            var factory = new TestFactory();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<TestInterceptor>(interceptor);
        }

        [Fact]
        public void CreateInstance_WithServiceProvider_PassesToCreateInstance()
        {
            var factory = new TestFactory();
            IServiceProvider serviceProvider = new FakeServiceProvider();
            var interceptor = factory.CreateInstance(serviceProvider);
            Assert.NotNull(interceptor);
        }

        #endregion

        #region Test Types

        private class TestFactory : InterceptorFactory
        {
            public TestFactory(params AspectPredicate[] predicates) : base(predicates) { }

            public override IInterceptor CreateInstance(IServiceProvider serviceProvider)
            {
                return new TestInterceptor();
            }
        }

        private class TestInterceptor : AbstractInterceptor
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
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
