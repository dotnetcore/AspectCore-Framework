using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class AspectValidationHandlerCollectionTests
    {
        #region Add

        [Fact]
        public void Add_WithNullHandler_ThrowsArgumentNullException()
        {
            var collection = new AspectValidationHandlerCollection();
            var ex = Assert.Throws<ArgumentNullException>(() => collection.Add(null));
            Assert.Equal("aspectValidationHandler", ex.ParamName);
        }

        [Fact]
        public void Add_WithValidHandler_IncreasesCount()
        {
            var collection = new AspectValidationHandlerCollection();
            collection.Add(new TestValidationHandler());
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void Add_ReturnsSameCollection_ForFluentChaining()
        {
            var collection = new AspectValidationHandlerCollection();
            var result = collection.Add(new TestValidationHandler());
            Assert.Same(collection, result);
        }

        [Fact]
        public void Add_SupportsFluentChaining()
        {
            var collection = new AspectValidationHandlerCollection();
            collection.Add(new TestValidationHandler())
                .Add(new AnotherValidationHandler());
            Assert.Equal(2, collection.Count);
        }

        #endregion

        #region Count

        [Fact]
        public void Count_NewCollection_IsZero()
        {
            var collection = new AspectValidationHandlerCollection();
            Assert.Equal(0, collection.Count);
        }

        #endregion

        #region Deduplication

        [Fact]
        public void Add_SameHandlerTypeTwice_DoesNotDuplicate()
        {
            var collection = new AspectValidationHandlerCollection();
            collection.Add(new TestValidationHandler());
            collection.Add(new TestValidationHandler());
            // HashSet with type-based equality comparer means same type is deduplicated
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void Add_DifferentHandlerTypes_BothAreAdded()
        {
            var collection = new AspectValidationHandlerCollection();
            collection.Add(new TestValidationHandler());
            collection.Add(new AnotherValidationHandler());
            Assert.Equal(2, collection.Count);
        }

        #endregion

        #region GetEnumerator

        [Fact]
        public void GetEnumerator_ReturnsAllAddedHandlers()
        {
            var collection = new AspectValidationHandlerCollection();
            collection.Add(new TestValidationHandler());
            collection.Add(new AnotherValidationHandler());

            var items = collection.ToList();
            Assert.Equal(2, items.Count);
        }

        [Fact]
        public void GetEnumerator_Generic_ReturnsIAspectValidationHandlerType()
        {
            var collection = new AspectValidationHandlerCollection();
            collection.Add(new TestValidationHandler());

            IEnumerator<IAspectValidationHandler> enumerator = collection.GetEnumerator();
            Assert.NotNull(enumerator);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsAllAddedHandlers()
        {
            var collection = new AspectValidationHandlerCollection();
            collection.Add(new TestValidationHandler());

            IEnumerable enumerable = collection;
            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }
            Assert.Single(items);
        }

        [Fact]
        public void GetEnumerator_EmptyCollection_ReturnsEmptyEnumerator()
        {
            var collection = new AspectValidationHandlerCollection();
            var items = collection.ToList();
            Assert.Empty(items);
        }

        #endregion

        #region Test Types

        private class TestValidationHandler : IAspectValidationHandler
        {
            public int Order => 0;

            public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
            {
                return next(context);
            }
        }

        private class AnotherValidationHandler : IAspectValidationHandler
        {
            public int Order => 1;

            public bool Invoke(AspectValidationContext context, AspectValidationDelegate next)
            {
                return next(context);
            }
        }

        #endregion
    }
}
