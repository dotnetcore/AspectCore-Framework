using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceContextTests
    {
        [Fact]
        public void DefaultConstructor_CreatesEmptyContextWithDefaultConfiguration()
        {
            var context = new ServiceContext();
            Assert.NotNull(context.Configuration);
            Assert.NotNull(context.Singletons);
            Assert.NotNull(context.Scopeds);
            Assert.NotNull(context.Transients);
            // Should have internal services registered
            Assert.True(context.Count > 0);
        }

        [Fact]
        public void Constructor_WithServices_AddsServices()
        {
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient)
            };
            var context = new ServiceContext(services);
            Assert.True(context.Count > 1); // internal services + our service
        }

        [Fact]
        public void Constructor_WithAspectConfiguration_UsesProvidedConfiguration()
        {
            var config = new AspectConfiguration();
            var context = new ServiceContext(config);
            Assert.Same(config, context.Configuration);
        }

        [Fact]
        public void Constructor_WithServicesAndConfiguration_UsesBoth()
        {
            var config = new AspectConfiguration();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparable), typeof(IComparable), Lifetime.Transient)
            };
            var context = new ServiceContext(services, config);
            Assert.Same(config, context.Configuration);
            Assert.True(context.Count > 1);
        }

        [Fact]
        public void Constructor_WithNullServices_DoesNotThrow()
        {
            var context = new ServiceContext((IEnumerable<ServiceDefinition>)null);
            Assert.NotNull(context.Configuration);
            Assert.True(context.Count > 0);
        }

        [Fact]
        public void Constructor_WithNullConfiguration_CreatesDefaultConfiguration()
        {
            var context = new ServiceContext((IAspectConfiguration)null);
            Assert.NotNull(context.Configuration);
        }

        [Fact]
        public void Add_AddsServiceDefinition()
        {
            var context = new ServiceContext();
            var initialCount = context.Count;
            context.Add(new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient));
            Assert.Equal(initialCount + 1, context.Count);
        }

        [Fact]
        public void Remove_RemovesServiceDefinition()
        {
            var context = new ServiceContext();
            var def = new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient);
            context.Add(def);
            Assert.True(context.Remove(def));
        }

        [Fact]
        public void Remove_WhenNotPresent_ReturnsFalse()
        {
            var context = new ServiceContext();
            var def = new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient);
            Assert.False(context.Remove(def));
        }

        [Fact]
        public void Contains_WithExistingServiceType_ReturnsTrue()
        {
            var context = new ServiceContext();
            // IAspectConfiguration is always registered as a singleton
            Assert.True(context.Contains(typeof(IAspectConfiguration)));
        }

        [Fact]
        public void Contains_WithNonExistingServiceType_ReturnsFalse()
        {
            var context = new ServiceContext();
            Assert.False(context.Contains(typeof(IFormattable)));
        }

        [Fact]
        public void GetEnumerator_ReturnsAllServiceDefinitions()
        {
            var context = new ServiceContext();
            var count = 0;
            foreach (var def in context)
            {
                count++;
            }
            Assert.Equal(context.Count, count);
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsAllServiceDefinitions()
        {
            var context = new ServiceContext();
            var count = 0;
            var enumerator = ((IEnumerable)context).GetEnumerator();
            while (enumerator.MoveNext())
            {
                count++;
            }
            Assert.Equal(context.Count, count);
        }

        [Fact]
        public void Configuration_DefaultConfiguration_IsNotNull()
        {
            var context = new ServiceContext();
            Assert.NotNull(context.Configuration);
        }

        [Fact]
        public void Singletons_HasCorrectLifetime()
        {
            var context = new ServiceContext();
            Assert.Equal(Lifetime.Singleton, context.Singletons.Lifetime);
        }

        [Fact]
        public void Scopeds_HasCorrectLifetime()
        {
            var context = new ServiceContext();
            Assert.Equal(Lifetime.Scoped, context.Scopeds.Lifetime);
        }

        [Fact]
        public void Transients_HasCorrectLifetime()
        {
            var context = new ServiceContext();
            Assert.Equal(Lifetime.Transient, context.Transients.Lifetime);
        }
    }
}
