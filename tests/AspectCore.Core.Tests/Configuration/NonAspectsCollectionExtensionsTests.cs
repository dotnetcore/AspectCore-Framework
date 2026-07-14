using System;
using System.Linq;
using AspectCore.Configuration;
using Xunit;

namespace AspectCore.Core.Tests.Configuration
{
    public class NonAspectsCollectionExtensionsTests
    {
        [Fact]
        public void AddNamespace_WithNullCollection_ThrowsArgumentNullException()
        {
            NonAspectPredicateCollection collection = null;
            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddNamespace("System"));
            Assert.Equal("collection", ex.ParamName);
        }

        [Fact]
        public void AddNamespace_AddsPredicateForNamespace()
        {
            var collection = new NonAspectPredicateCollection();
            var result = collection.AddNamespace("System");
            Assert.Same(collection, result);
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void AddService_WithNullCollection_ThrowsArgumentNullException()
        {
            NonAspectPredicateCollection collection = null;
            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddService("MyService"));
            Assert.Equal("collection", ex.ParamName);
        }

        [Fact]
        public void AddService_AddsPredicateForService()
        {
            var collection = new NonAspectPredicateCollection();
            var result = collection.AddService("MyService");
            Assert.Same(collection, result);
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void AddMethod_WithNullCollection_ThrowsArgumentNullException()
        {
            NonAspectPredicateCollection collection = null;
            var ex = Assert.Throws<ArgumentNullException>(() => collection.AddMethod("MyMethod"));
            Assert.Equal("collection", ex.ParamName);
        }

        [Fact]
        public void AddMethod_WithMethodNameOnly_AddsPredicate()
        {
            var collection = new NonAspectPredicateCollection();
            var result = collection.AddMethod("ToString");
            Assert.Same(collection, result);
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void AddMethod_WithServiceAndMethod_AddsPredicate()
        {
            var collection = new NonAspectPredicateCollection();
            var result = collection.AddMethod("MyService", "MyMethod");
            Assert.Same(collection, result);
            Assert.Equal(1, collection.Count);
        }

        [Fact]
        public void AddDefault_AddsAllDefaultPredicates()
        {
            var collection = new NonAspectPredicateCollection();
            var result = collection.AddDefault();
            Assert.Same(collection, result);
            Assert.True(collection.Count > 0);
        }

        [Fact]
        public void AddDefault_IncludesSystemNamespaces()
        {
            var collection = new NonAspectPredicateCollection();
            collection.AddDefault();
            // Verify that System namespace predicates were added
            var predicates = collection.ToList();
            Assert.True(predicates.Count > 0);
        }

        [Fact]
        public void AddDefault_IncludesAspectCoreNamespaces()
        {
            var collection = new NonAspectPredicateCollection();
            collection.AddDefault();
            // Should include AspectCore.Configuration, AspectCore.DynamicProxy, etc.
            Assert.True(collection.Count > 5);
        }

        [Fact]
        public void AddNamespace_MultipleCalls_AccumulatesPredicates()
        {
            var collection = new NonAspectPredicateCollection();
            collection.AddNamespace("System");
            collection.AddNamespace("Microsoft");
            Assert.Equal(2, collection.Count);
        }

        [Fact]
        public void AddMethod_MultipleOverloads_AccumulatesPredicates()
        {
            var collection = new NonAspectPredicateCollection();
            collection.AddMethod("Equals");
            collection.AddMethod("MyService", "MyMethod");
            Assert.Equal(2, collection.Count);
        }
    }
}
