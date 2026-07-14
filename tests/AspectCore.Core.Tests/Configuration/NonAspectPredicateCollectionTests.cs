using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.Configuration;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class NonAspectPredicateCollectionTests
    {
        #region Add

        [Fact]
        public void Add_WithNullPredicate_DoesNotThrow()
        {
            var collection = new NonAspectPredicateCollection();
            // NonAspectPredicateCollection.Add does not validate null
            collection.Add(null);
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void Add_WithValidPredicate_IncreasesCount()
        {
            var collection = new NonAspectPredicateCollection();
            AspectPredicate predicate = method => true;
            collection.Add(predicate);
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void Add_WithMultiplePredicates_IncreasesCountAccordingly()
        {
            var collection = new NonAspectPredicateCollection();
            collection.Add(method => true);
            collection.Add(method => false);
            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void Add_ReturnsSameCollection_ForFluentChaining()
        {
            var collection = new NonAspectPredicateCollection();
            var result = collection.Add(method => true);
            Assert.Same(collection, result);
        }

        [Fact]
        public void Add_SupportsFluentChaining()
        {
            var collection = new NonAspectPredicateCollection();
            collection.Add(method => true)
                .Add(method => false)
                .Add(method => true);
            Assert.Equal(3, collection.Count);
        }

        #endregion

        #region Count

        [Fact]
        public void Count_NewCollection_IsZero()
        {
            var collection = new NonAspectPredicateCollection();
            Assert.Equal(0, collection.Count);
        }

        #endregion

        #region GetEnumerator

        [Fact]
        public void GetEnumerator_ReturnsAllAddedPredicates()
        {
            var collection = new NonAspectPredicateCollection();
            AspectPredicate predicate1 = method => true;
            AspectPredicate predicate2 = method => false;
            collection.Add(predicate1);
            collection.Add(predicate2);

            var items = collection.ToList();
            Assert.Equal(2, items.Count);
            Assert.Same(predicate1, items[0]);
            Assert.Same(predicate2, items[1]);
        }

        [Fact]
        public void GetEnumerator_Generic_ReturnsAspectPredicateType()
        {
            var collection = new NonAspectPredicateCollection();
            collection.Add(method => true);

            IEnumerator<AspectPredicate> enumerator = collection.GetEnumerator();
            Assert.NotNull(enumerator);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsAllAddedPredicates()
        {
            var collection = new NonAspectPredicateCollection();
            AspectPredicate predicate = method => true;
            collection.Add(predicate);

            IEnumerable enumerable = collection;
            var items = new List<object>();
            foreach (var item in enumerable)
            {
                items.Add(item);
            }
            Assert.Single(items);
            Assert.Same(predicate, items[0]);
        }

        [Fact]
        public void GetEnumerator_EmptyCollection_ReturnsEmptyEnumerator()
        {
            var collection = new NonAspectPredicateCollection();
            var items = collection.ToList();
            Assert.Empty(items);
        }

        #endregion

        #region Predicate Evaluation

        [Fact]
        public void AddedPredicates_CanBeEvaluated()
        {
            var collection = new NonAspectPredicateCollection();
            AspectPredicate predicate = method => method.Name == "Foo";
            collection.Add(predicate);

            var method = typeof(TestService).GetMethod(nameof(TestService.Foo));
            var items = collection.ToList();
            Assert.True(items[0](method));
        }

        [Fact]
        public void AddedPredicates_CanBeEvaluated_WithNonMatchingMethod()
        {
            var collection = new NonAspectPredicateCollection();
            AspectPredicate predicate = method => method.Name == "Foo";
            collection.Add(predicate);

            var method = typeof(TestService).GetMethod(nameof(TestService.Bar));
            var items = collection.ToList();
            Assert.False(items[0](method));
        }

        #endregion

        #region Test Types

        private class TestService
        {
            public void Foo() { }

            public void Bar() { }
        }

        #endregion
    }
}
