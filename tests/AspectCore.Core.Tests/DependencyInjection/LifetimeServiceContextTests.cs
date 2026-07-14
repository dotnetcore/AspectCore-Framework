using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class LifetimeServiceContextTests
    {
        [Fact]
        public void Constructor_SetsLifetime()
        {
            var collection = new List<ServiceDefinition>();
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            Assert.Equal(Lifetime.Singleton, context.Lifetime);
        }

        [Fact]
        public void Count_ReturnsOnlyMatchingLifetime()
        {
            var collection = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Singleton),
                new TypeServiceDefinition(typeof(IComparable), typeof(IComparable), Lifetime.Scoped),
                new TypeServiceDefinition(typeof(IFormattable), typeof(IFormattable), Lifetime.Singleton),
            };
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            Assert.Equal(2, context.Count);
        }

        [Fact]
        public void Count_WhenNoMatchingLifetime_ReturnsZero()
        {
            var collection = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Scoped),
            };
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            Assert.Equal(0, context.Count);
        }

        [Fact]
        public void Add_WithMatchingLifetime_AddsToCollection()
        {
            var collection = new List<ServiceDefinition>();
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Singleton));
            Assert.Single(collection);
        }

        [Fact]
        public void Add_WithNonMatchingLifetime_DoesNotAddToCollection()
        {
            var collection = new List<ServiceDefinition>();
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Scoped));
            Assert.Empty(collection);
        }

        [Fact]
        public void Contains_WithMatchingServiceTypeAndLifetime_ReturnsTrue()
        {
            var collection = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Singleton),
            };
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            Assert.True(context.Contains(typeof(IDisposable)));
        }

        [Fact]
        public void Contains_WithMatchingServiceTypeButDifferentLifetime_ReturnsFalse()
        {
            var collection = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Scoped),
            };
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            Assert.False(context.Contains(typeof(IDisposable)));
        }

        [Fact]
        public void Contains_WithNonExistingServiceType_ReturnsFalse()
        {
            var collection = new List<ServiceDefinition>();
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            Assert.False(context.Contains(typeof(IDisposable)));
        }

        [Fact]
        public void GetEnumerator_ReturnsOnlyMatchingLifetime()
        {
            var collection = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Singleton),
                new TypeServiceDefinition(typeof(IComparable), typeof(IComparable), Lifetime.Scoped),
                new TypeServiceDefinition(typeof(IFormattable), typeof(IFormattable), Lifetime.Singleton),
            };
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            var items = context.ToList();
            Assert.Equal(2, items.Count);
            Assert.All(items, x => Assert.Equal(Lifetime.Singleton, x.Lifetime));
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsOnlyMatchingLifetime()
        {
            var collection = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Singleton),
                new TypeServiceDefinition(typeof(IComparable), typeof(IComparable), Lifetime.Scoped),
            };
            var context = new LifetimeServiceContext(collection, Lifetime.Singleton);
            var count = 0;
            var enumerator = ((IEnumerable)context).GetEnumerator();
            while (enumerator.MoveNext())
            {
                count++;
            }
            Assert.Equal(1, count);
        }
    }
}
