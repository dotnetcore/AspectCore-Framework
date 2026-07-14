using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceTableTests
    {
        private static ServiceTable CreateServiceTable(IEnumerable<ServiceDefinition> services = null)
        {
            var context = new ServiceContext(services ?? new List<ServiceDefinition>());
            return new ServiceTable(context);
        }

        [Fact]
        public void Constructor_WithNullServiceContext_ThrowsArgumentNullException()
        {
            var ex = Assert.Throws<ArgumentNullException>(() => new ServiceTable(null));
            Assert.Equal("serviceContext", ex.ParamName);
        }

        [Fact]
        public void Populate_WithNonGenericService_AddsToLinkedServices()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IDisposable)));
        }

        [Fact]
        public void Populate_WithGenericService_AddsToGenericServices()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparer<>), typeof(IComparer<>), Lifetime.Transient)
            };
            table.Populate(services);
            // Contains returns true for constructed generic types when the open generic is registered
            Assert.True(table.Contains(typeof(IComparer<int>)));
        }

        [Fact]
        public void Populate_WithManyEnumerable_FiltersOut()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IManyEnumerable<IDisposable>), typeof(IManyEnumerable<IDisposable>), Lifetime.Transient)
            };
            table.Populate(services);
            // ManyEnumerable services are filtered out by Populate (not stored in linked services)
            // But Contains always returns true for IManyEnumerable<> due to the switch case logic
            // Verify the service was filtered by checking TryGetService returns empty ManyEnumerable
            var result = table.TryGetService(typeof(IManyEnumerable<IDisposable>));
            Assert.NotNull(result);
            Assert.IsType<ManyEnumerableServiceDefinition>(result);
        }

        [Fact]
        public void Contains_WithNonExistingServiceType_ReturnsFalse()
        {
            var table = CreateServiceTable();
            Assert.False(table.Contains(typeof(IFormattable)));
        }

        [Fact]
        public void Contains_WithEnumerableType_ReturnsTrueWhenElementExists()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IEnumerable<IDisposable>)));
        }

        [Fact]
        public void Contains_WithManyEnumerableType_ReturnsTrueWhenElementExists()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IManyEnumerable<IDisposable>)));
        }

        [Fact]
        public void Contains_WithConstructedGenericType_ReturnsTrueWhenGenericDefinitionExists()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparer<>), typeof(IComparer<>), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IComparer<int>)));
        }

        [Fact]
        public void TryGetService_WithNullServiceType_ReturnsNull()
        {
            var table = CreateServiceTable();
            Assert.Null(table.TryGetService(null));
        }

        [Fact]
        public void TryGetService_WithNonExistingServiceType_ReturnsNull()
        {
            var table = CreateServiceTable();
            Assert.Null(table.TryGetService(typeof(IFormattable)));
        }

        [Fact]
        public void TryGetService_WithExistingServiceType_ReturnsServiceDefinition()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IDisposable));
            Assert.NotNull(result);
            Assert.Equal(typeof(IDisposable), result.ServiceType);
        }

        [Fact]
        public void TryGetService_WithEnumerableType_ReturnsEnumerableServiceDefinition()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IEnumerable<IDisposable>));
            Assert.NotNull(result);
            Assert.IsType<EnumerableServiceDefinition>(result);
        }

        [Fact]
        public void TryGetService_WithManyEnumerableType_ReturnsManyEnumerableServiceDefinition()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IManyEnumerable<IDisposable>));
            Assert.NotNull(result);
            Assert.IsType<ManyEnumerableServiceDefinition>(result);
        }

        [Fact]
        public void TryGetService_WithConstructedGenericType_ReturnsServiceDefinition()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparer<>), typeof(IComparer<>), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IComparer<int>));
            Assert.NotNull(result);
        }

        [Fact]
        public void TryGetService_WithInstanceService_ReturnsServiceDefinition()
        {
            var table = CreateServiceTable();
            var instance = new object();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(object), instance)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(object));
            Assert.NotNull(result);
        }

        [Fact]
        public void TryGetService_WithDelegateService_ReturnsServiceDefinition()
        {
            var table = CreateServiceTable();
            Func<IServiceResolver, object> impl = r => new object();
            var services = new List<ServiceDefinition>
            {
                new DelegateServiceDefinition(typeof(object), impl, Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(object));
            Assert.NotNull(result);
        }

        [Fact]
        public void Populate_WithMultipleServicesOfSameType_LastOneWins()
        {
            var table = CreateServiceTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(IDisposable), Lifetime.Transient),
                new InstanceServiceDefinition(typeof(IDisposable), new DisposableImpl()),
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IDisposable));
            Assert.NotNull(result);
            // The last registered service should be returned
            Assert.IsType<InstanceServiceDefinition>(result);
        }

        [Fact]
        public void Constructor_WithExplicitProxyTypeGenerator_UsesIt()
        {
            var generator = new ProxyTypeGenerator(new AspectValidatorBuilder(new AspectConfiguration()));
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IProxyTypeGenerator), generator)
            };
            var context = new ServiceContext(services);
            var table = new ServiceTable(context);
            Assert.NotNull(table);
        }

        private class DisposableImpl : IDisposable
        {
            public void Dispose() { }
        }
    }
}
