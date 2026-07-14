using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class InterceptorCollectionTests
    {
        #region Add

        [Fact]
        public void Add_WithNullInterceptorFactory_ThrowsArgumentNullException()
        {
            var collection = new InterceptorCollection();
            var ex = Assert.Throws<ArgumentNullException>(() => collection.Add(null));
            Assert.Equal("interceptorFactory", ex.ParamName);
        }

        [Fact]
        public void Add_WithValidInterceptorFactory_IncreasesCount()
        {
            var collection = new InterceptorCollection();
            collection.Add(new TestInterceptorFactory());
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void Add_WithMultipleFactories_IncreasesCountAccordingly()
        {
            var collection = new InterceptorCollection();
            collection.Add(new TestInterceptorFactory());
            collection.Add(new TestInterceptorFactory());
            collection.Add(new TestInterceptorFactory());
            Assert.Equal(3, collection.Count);
        }

        [Fact]
        public void Add_ReturnsSameCollection_ForFluentChaining()
        {
            var collection = new InterceptorCollection();
            var result = collection.Add(new TestInterceptorFactory());
            Assert.Same(collection, result);
        }

        [Fact]
        public void Add_SupportsFluentChaining()
        {
            var collection = new InterceptorCollection();
            collection.Add(new TestInterceptorFactory())
                .Add(new TestInterceptorFactory())
                .Add(new TestInterceptorFactory());
            Assert.Equal(3, collection.Count);
        }

        #endregion

        #region Count

        [Fact]
        public void Count_NewCollection_IsZero()
        {
            var collection = new InterceptorCollection();
            Assert.Equal(0, collection.Count);
        }

        #endregion

        #region GetEnumerator

        [Fact]
        public void GetEnumerator_ReturnsAllAddedFactories()
        {
            var collection = new InterceptorCollection();
            var factory1 = new TestInterceptorFactory();
            var factory2 = new TestInterceptorFactory();
            collection.Add(factory1);
            collection.Add(factory2);

            var items = collection.ToList();
            Assert.Equal(2, items.Count);
            Assert.Same(factory1, items[0]);
            Assert.Same(factory2, items[1]);
        }

        [Fact]
        public void GetEnumerator_Generic_ReturnsInterceptorFactoryType()
        {
            var collection = new InterceptorCollection();
            collection.Add(new TestInterceptorFactory());

            IEnumerator<InterceptorFactory> enumerator = collection.GetEnumerator();
            Assert.NotNull(enumerator);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsAllAddedFactories()
        {
            var collection = new InterceptorCollection();
            var factory = new TestInterceptorFactory();
            collection.Add(factory);

            IEnumerable enumerable = collection;
            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }
            Assert.Single(items);
            Assert.Same(factory, items[0]);
        }

        [Fact]
        public void GetEnumerator_EmptyCollection_ReturnsEmptyEnumerator()
        {
            var collection = new InterceptorCollection();
            var items = collection.ToList();
            Assert.Empty(items);
        }

        #endregion

        #region Test Types

        private class TestInterceptorFactory : InterceptorFactory
        {
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

        #endregion
    }
}
