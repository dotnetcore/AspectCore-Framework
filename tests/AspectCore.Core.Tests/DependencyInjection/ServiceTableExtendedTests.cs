using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceTableExtendedTests
    {
        [Fact]
        public void Constructor_WithSourceGeneratorEngine_CreatesSourceGeneratedProxyTypeGenerator()
        {
            var options = new ProxyEngineOptions { Engine = ProxyEngine.SourceGenerator };
            var registry = new FakeRegistry();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(ProxyEngineOptions), options),
                new InstanceServiceDefinition(typeof(ISourceGeneratedProxyRegistry), registry),
            };
            var context = new ServiceContext(services);
            var table = new ServiceTable(context);
            Assert.NotNull(table);
        }

        [Fact]
        public void Contains_WithConstructedGenericTypeMatchingRegisteredGeneric_ReturnsTrue()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparer<>), typeof(IComparer<>), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IComparer<int>)));
        }

        [Fact]
        public void Contains_WithConstructedGenericTypeNotMatching_ReturnsFalse()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparer<>), typeof(IComparer<>), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.False(table.Contains(typeof(IFormattable)));
        }

        [Fact]
        public void TryGetService_WithConstructedGenericType_ReturnsService()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparer<>), typeof(ComparerImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IComparer<int>));
            Assert.NotNull(result);
        }

        [Fact]
        public void TryGetService_WithEnumerableType_ReturnsEnumerableServiceDefinition()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IEnumerable<IDisposable>));
            Assert.NotNull(result);
            Assert.IsType<EnumerableServiceDefinition>(result);
        }

        [Fact]
        public void TryGetService_WithManyEnumerableType_ReturnsManyEnumerableServiceDefinition()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IManyEnumerable<IDisposable>));
            Assert.NotNull(result);
            Assert.IsType<ManyEnumerableServiceDefinition>(result);
        }

        [Fact]
        public void TryGetService_WithGenericService_ReturnsProxyServiceDefinition()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IGenericService<>), typeof(GenericServiceImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IGenericService<int>));
            Assert.NotNull(result);
        }

        [Fact]
        public void TryGetService_WithNonGenericService_ReturnsProxyServiceDefinition()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ITestService), typeof(TestServiceImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(ITestService));
            Assert.NotNull(result);
        }

        [Fact]
        public void Populate_WithInstanceService_AddsToLinkedServices()
        {
            var table = CreateTable();
            var instance = new DisposableImpl();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IDisposable), instance)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IDisposable)));
        }

        [Fact]
        public void Populate_WithDelegateService_AddsToLinkedServices()
        {
            var table = CreateTable();
            Func<IServiceResolver, object> impl = r => new DisposableImpl();
            var services = new List<ServiceDefinition>
            {
                new DelegateServiceDefinition(typeof(IDisposable), impl, Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IDisposable)));
        }

        [Fact]
        public void Populate_WithMultipleServicesOfSameType_LastOneIsReturned()
        {
            var table = CreateTable();
            var instance1 = new DisposableImpl();
            var instance2 = new DisposableImpl();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IDisposable), instance1),
                new InstanceServiceDefinition(typeof(IDisposable), instance2),
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IDisposable));
            Assert.NotNull(result);
        }

        private static ServiceTable CreateTable()
        {
            var context = new ServiceContext();
            return new ServiceTable(context);
        }

        public interface ITestService
        {
            void DoSomething();
        }

        public class TestServiceImpl : ITestService
        {
            public void DoSomething() { }
        }

        public interface IGenericService<T>
        {
            T GetValue();
        }

        public class GenericServiceImpl<T> : IGenericService<T>
        {
            public T GetValue() => default;
        }

        public class ComparerImpl<T> : IComparer<T>
        {
            public int Compare(T x, T y) => 0;
        }

        public class DisposableImpl : IDisposable
        {
            public void Dispose() { }
        }

        private class FakeRegistry : ISourceGeneratedProxyRegistry
        {
            public bool TryGetProxyType(Type serviceType, Type implementationType, SourceGeneratedProxyKind kind, out Type proxyType)
            {
                proxyType = null;
                return false;
            }
        }
    }
}
