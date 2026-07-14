using System;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class InterceptorCollectionExtensionsTests
    {
        #region AddTyped(Type, params AspectPredicate[])

        [Fact]
        public void AddTyped_WithType_AddsTypeInterceptorFactory()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped(typeof(TestInterceptor));

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<TypeInterceptorFactory>(factory);
        }

        [Fact]
        public void AddTyped_WithType_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();

            var result = collection.AddTyped(typeof(TestInterceptor));

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddTyped_WithType_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => true;

            collection.AddTyped(typeof(TestInterceptor), predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddTyped_WithType_NoPredicates_HasEmptyPredicates()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped(typeof(TestInterceptor));

            var factory = collection.Single();
            Assert.Empty(factory.Predicates);
        }

        [Fact]
        public void AddTyped_WithType_CanCreateInterceptorInstance()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped(typeof(TestInterceptor));

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<TestInterceptor>(interceptor);
        }

        [Fact]
        public void AddTyped_WithNullCollection_ThrowsArgumentNullException()
        {
            InterceptorCollection collection = null;

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped(typeof(TestInterceptor)));
            Assert.Equal("interceptorCollection", ex.ParamName);
        }

        [Fact]
        public void AddTyped_WithNullType_ThrowsArgumentNullException()
        {
            var collection = new InterceptorCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped((Type)null));
            Assert.Equal("interceptorType", ex.ParamName);
        }

        #endregion

        #region AddTyped(Type, object[], params AspectPredicate[])

        [Fact]
        public void AddTyped_WithTypeAndArgs_AddsTypeInterceptorFactory()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped(typeof(TestInterceptorWithArgs), new object[] { 42 });

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<TypeInterceptorFactory>(factory);
        }

        [Fact]
        public void AddTyped_WithTypeAndArgs_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();

            var result = collection.AddTyped(typeof(TestInterceptorWithArgs), new object[] { 42 });

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddTyped_WithTypeAndArgs_CanCreateInterceptorInstanceWithArgs()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped(typeof(TestInterceptorWithArgs), new object[] { 42 });

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            var typed = Assert.IsType<TestInterceptorWithArgs>(interceptor);
            Assert.Equal(42, typed.Value);
        }

        [Fact]
        public void AddTyped_WithTypeAndArgs_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => true;

            collection.AddTyped(typeof(TestInterceptorWithArgs), new object[] { 42 }, predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddTyped_WithTypeAndArgs_NullArgs_CreatesInterceptorWithDefaultConstructor()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped(typeof(TestInterceptor), null);

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<TestInterceptor>(interceptor);
        }

        [Fact]
        public void AddTyped_WithTypeAndArgs_WithNullCollection_ThrowsArgumentNullException()
        {
            InterceptorCollection collection = null;

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped(typeof(TestInterceptorWithArgs), new object[] { 42 }));
            Assert.Equal("interceptorCollection", ex.ParamName);
        }

        [Fact]
        public void AddTyped_WithTypeAndArgs_WithNullPredicates_ThrowsArgumentNullException()
        {
            var collection = new InterceptorCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped(typeof(TestInterceptorWithArgs), new object[] { 42 }, null));
            Assert.Equal("predicates", ex.ParamName);
        }

        [Fact]
        public void AddTyped_WithTypeAndArgs_WithNullType_ThrowsArgumentNullException()
        {
            var collection = new InterceptorCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped((Type)null, new object[] { 42 }));
            Assert.Equal("interceptorType", ex.ParamName);
        }

        #endregion

        #region AddTyped<TInterceptor>(params AspectPredicate[])

        [Fact]
        public void AddTyped_Generic_AddsTypeInterceptorFactory()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped<TestInterceptor>();

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<TypeInterceptorFactory>(factory);
        }

        [Fact]
        public void AddTyped_Generic_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();

            var result = collection.AddTyped<TestInterceptor>();

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddTyped_Generic_WithPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => true;

            collection.AddTyped<TestInterceptor>(predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddTyped_Generic_CanCreateInterceptorInstance()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped<TestInterceptor>();

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<TestInterceptor>(interceptor);
        }

        [Fact]
        public void AddTyped_Generic_WithNullCollection_ThrowsArgumentNullException()
        {
            InterceptorCollection collection = null;

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped<TestInterceptor>());
            Assert.Equal("interceptorCollection", ex.ParamName);
        }

        #endregion

        #region AddTyped<TInterceptor>(object[], params AspectPredicate[])

        [Fact]
        public void AddTyped_Generic_WithArgs_AddsTypeInterceptorFactory()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped<TestInterceptorWithArgs>(new object[] { 42 });

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<TypeInterceptorFactory>(factory);
        }

        [Fact]
        public void AddTyped_Generic_WithArgs_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();

            var result = collection.AddTyped<TestInterceptorWithArgs>(new object[] { 42 });

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddTyped_Generic_WithArgs_CanCreateInterceptorInstanceWithArgs()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped<TestInterceptorWithArgs>(new object[] { 42 });

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            var typed = Assert.IsType<TestInterceptorWithArgs>(interceptor);
            Assert.Equal(42, typed.Value);
        }

        [Fact]
        public void AddTyped_Generic_WithArgs_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => true;

            collection.AddTyped<TestInterceptorWithArgs>(new object[] { 42 }, predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddTyped_Generic_WithArgs_WithNullCollection_ThrowsArgumentNullException()
        {
            InterceptorCollection collection = null;

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped<TestInterceptorWithArgs>(new object[] { 42 }));
            Assert.Equal("interceptorCollection", ex.ParamName);
        }

        [Fact]
        public void AddTyped_Generic_WithArgs_WithNullPredicates_ThrowsArgumentNullException()
        {
            var collection = new InterceptorCollection();

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddTyped<TestInterceptorWithArgs>(new object[] { 42 }, null));
            Assert.Equal("predicates", ex.ParamName);
        }

        #endregion

        #region AddServiced(Type, params AspectPredicate[])

        [Fact]
        public void AddServiced_WithType_AddsServiceInterceptorFactory()
        {
            var collection = new InterceptorCollection();

            collection.AddServiced(typeof(TestInterceptor));

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<ServiceInterceptorFactory>(factory);
        }

        [Fact]
        public void AddServiced_WithType_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();

            var result = collection.AddServiced(typeof(TestInterceptor));

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddServiced_WithType_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => true;

            collection.AddServiced(typeof(TestInterceptor), predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddServiced_WithType_NoPredicates_HasEmptyPredicates()
        {
            var collection = new InterceptorCollection();

            collection.AddServiced(typeof(TestInterceptor));

            var factory = collection.Single();
            Assert.Empty(factory.Predicates);
        }

        [Fact]
        public void AddServiced_WithType_CanCreateServiceInterceptorAttribute()
        {
            var collection = new InterceptorCollection();

            collection.AddServiced(typeof(TestInterceptor));

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<ServiceInterceptorAttribute>(interceptor);
        }

        [Fact]
        public void AddServiced_WithNullCollection_ThrowsArgumentNullException()
        {
            InterceptorCollection collection = null;

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddServiced(typeof(TestInterceptor)));
            Assert.Equal("interceptorCollection", ex.ParamName);
        }

        #endregion

        #region AddServiced<TInterceptor>(params AspectPredicate[])

        [Fact]
        public void AddServiced_Generic_AddsServiceInterceptorFactory()
        {
            var collection = new InterceptorCollection();

            collection.AddServiced<TestInterceptor>();

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<ServiceInterceptorFactory>(factory);
        }

        [Fact]
        public void AddServiced_Generic_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();

            var result = collection.AddServiced<TestInterceptor>();

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddServiced_Generic_WithPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => true;

            collection.AddServiced<TestInterceptor>(predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddServiced_Generic_CanCreateServiceInterceptorAttribute()
        {
            var collection = new InterceptorCollection();

            collection.AddServiced<TestInterceptor>();

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<ServiceInterceptorAttribute>(interceptor);
        }

        #endregion

        #region AddDelegate(Func<AspectDelegate, AspectDelegate>, int, params AspectPredicate[])

        [Fact]
        public void AddDelegate_WithDelegateAndOrder_AddsDelegateInterceptorFactory()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            collection.AddDelegate(aspectDelegate, 5);

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<DelegateInterceptorFactory>(factory);
        }

        [Fact]
        public void AddDelegate_WithDelegateAndOrder_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            var result = collection.AddDelegate(aspectDelegate, 5);

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddDelegate_WithDelegateAndOrder_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            AspectPredicate predicate = method => true;

            collection.AddDelegate(aspectDelegate, 5, predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddDelegate_WithDelegateAndOrder_CanCreateDelegateInterceptor()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            collection.AddDelegate(aspectDelegate, 5);

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<DelegateInterceptor>(interceptor);
        }

        [Fact]
        public void AddDelegate_WithDelegateAndOrder_CreatedInterceptorHasCorrectOrder()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            collection.AddDelegate(aspectDelegate, 7);

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            var delegateInterceptor = Assert.IsType<DelegateInterceptor>(interceptor);
            Assert.Equal(7, delegateInterceptor.Order);
        }

        [Fact]
        public void AddDelegate_WithDelegateAndOrder_WithNullCollection_ThrowsArgumentNullException()
        {
            InterceptorCollection collection = null;
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddDelegate(aspectDelegate, 5));
            Assert.Equal("interceptorCollection", ex.ParamName);
        }

        #endregion

        #region AddDelegate(Func<AspectDelegate, AspectDelegate>, params AspectPredicate[])

        [Fact]
        public void AddDelegate_WithDelegate_NoOrder_AddsDelegateInterceptorFactory()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            collection.AddDelegate(aspectDelegate);

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<DelegateInterceptorFactory>(factory);
        }

        [Fact]
        public void AddDelegate_WithDelegate_NoOrder_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            var result = collection.AddDelegate(aspectDelegate);

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddDelegate_WithDelegate_NoOrder_DefaultsToOrderZero()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);

            collection.AddDelegate(aspectDelegate);

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            var delegateInterceptor = Assert.IsType<DelegateInterceptor>(interceptor);
            Assert.Equal(0, delegateInterceptor.Order);
        }

        [Fact]
        public void AddDelegate_WithDelegate_NoOrder_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            Func<AspectDelegate, AspectDelegate> aspectDelegate = next => context => next(context);
            AspectPredicate predicate = method => true;

            collection.AddDelegate(aspectDelegate, predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        #endregion

        #region AddDelegate(Func<AspectContext, AspectDelegate, Task>, int, params AspectPredicate[])

        [Fact]
        public void AddDelegate_WithContextDelegateAndOrder_AddsDelegateInterceptorFactory()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);

            collection.AddDelegate(aspectDelegate, 5);

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<DelegateInterceptorFactory>(factory);
        }

        [Fact]
        public void AddDelegate_WithContextDelegateAndOrder_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);

            var result = collection.AddDelegate(aspectDelegate, 5);

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddDelegate_WithContextDelegateAndOrder_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);
            AspectPredicate predicate = method => true;

            collection.AddDelegate(aspectDelegate, 5, predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        [Fact]
        public void AddDelegate_WithContextDelegateAndOrder_CanCreateDelegateInterceptor()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);

            collection.AddDelegate(aspectDelegate, 5);

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            Assert.IsType<DelegateInterceptor>(interceptor);
        }

        [Fact]
        public void AddDelegate_WithContextDelegateAndOrder_CreatedInterceptorHasCorrectOrder()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);

            collection.AddDelegate(aspectDelegate, 3);

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            var delegateInterceptor = Assert.IsType<DelegateInterceptor>(interceptor);
            Assert.Equal(3, delegateInterceptor.Order);
        }

        #endregion

        #region AddDelegate(Func<AspectContext, AspectDelegate, Task>, params AspectPredicate[])

        [Fact]
        public void AddDelegate_WithContextDelegate_NoOrder_AddsDelegateInterceptorFactory()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);

            collection.AddDelegate(aspectDelegate);

            Assert.Equal(1, collection.Count);
            var factory = collection.Single();
            Assert.IsType<DelegateInterceptorFactory>(factory);
        }

        [Fact]
        public void AddDelegate_WithContextDelegate_NoOrder_ReturnsSameCollection()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);

            var result = collection.AddDelegate(aspectDelegate);

            Assert.Same(collection, result);
        }

        [Fact]
        public void AddDelegate_WithContextDelegate_NoOrder_DefaultsToOrderZero()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);

            collection.AddDelegate(aspectDelegate);

            var factory = collection.Single();
            var interceptor = factory.CreateInstance(null);
            var delegateInterceptor = Assert.IsType<DelegateInterceptor>(interceptor);
            Assert.Equal(0, delegateInterceptor.Order);
        }

        [Fact]
        public void AddDelegate_WithContextDelegate_NoOrder_AndPredicates_StoresPredicates()
        {
            var collection = new InterceptorCollection();
            Func<AspectContext, AspectDelegate, Task> aspectDelegate = (context, next) => next(context);
            AspectPredicate predicate = method => true;

            collection.AddDelegate(aspectDelegate, predicate);

            var factory = collection.Single();
            Assert.Single(factory.Predicates);
        }

        #endregion

        #region Multiple Additions

        [Fact]
        public void MultipleAdditions_IncreasesCount()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped<TestInterceptor>();
            collection.AddServiced<TestInterceptor>();
            collection.AddDelegate(next => context => next(context));

            Assert.Equal(3, collection.Count);
        }

        [Fact]
        public void MultipleAdditions_ChainUsingFluentApi()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped<TestInterceptor>()
                .AddServiced<TestInterceptor>()
                .AddDelegate(next => context => next(context));

            Assert.Equal(3, collection.Count);
        }

        #endregion

        #region Predicate Evaluation

        [Fact]
        public void Factory_WithPredicate_CanCreated_ReturnsTrueForMatchingMethod()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => method.Name == "Foo";

            collection.AddTyped<TestInterceptor>(predicate);

            var factory = collection.Single();
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));
            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void Factory_WithPredicate_CanCreated_ReturnsFalseForNonMatchingMethod()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate = method => method.Name == "Foo";

            collection.AddTyped<TestInterceptor>(predicate);

            var factory = collection.Single();
            var method = typeof(TestService).GetMethod(nameof(TestService.Bar));
            Assert.False(factory.CanCreated(method));
        }

        [Fact]
        public void Factory_WithoutPredicate_CanCreated_ReturnsTrueForAnyMethod()
        {
            var collection = new InterceptorCollection();

            collection.AddTyped<TestInterceptor>();

            var factory = collection.Single();
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));
            Assert.True(factory.CanCreated(method));
        }

        [Fact]
        public void Factory_WithMultiplePredicates_CanCreated_ReturnsTrueWhenAnyMatches()
        {
            var collection = new InterceptorCollection();
            AspectPredicate predicate1 = method => method.Name == "NonExistent";
            AspectPredicate predicate2 = method => method.Name == "Foo";

            collection.AddTyped<TestInterceptor>(predicate1, predicate2);

            var factory = collection.Single();
            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));
            Assert.True(factory.CanCreated(method));
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

        private class TestInterceptorWithArgs : AbstractInterceptor
        {
            public int Value { get; }

            public TestInterceptorWithArgs(int value)
            {
                Value = value;
            }

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

        #endregion
    }
}
