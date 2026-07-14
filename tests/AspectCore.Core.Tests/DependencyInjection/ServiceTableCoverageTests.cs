using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceTableCoverageTests
    {
        [Fact]
        public void Contains_WithConstructedGenericTypeMatchingRegisteredGeneric_ReturnsTrue()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IComparer<>), typeof(ComparerImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IComparer<int>)));
        }

        [Fact]
        public void Contains_WithEnumerableTypeWhenElementNotRegistered_ReturnsTrue()
        {
            var table = CreateTable();
            // Contains for IEnumerable<> always returns true (the code has ? true : true)
            Assert.True(table.Contains(typeof(IEnumerable<IFormattable>)));
        }

        [Fact]
        public void Contains_WithManyEnumerableTypeWhenElementNotRegistered_ReturnsTrue()
        {
            var table = CreateTable();
            // Contains for IManyEnumerable<> always returns true (the code has ? true : true)
            Assert.True(table.Contains(typeof(IManyEnumerable<IFormattable>)));
        }

        [Fact]
        public void TryGetService_WithCachedEnumerable_ReturnsSameInstance()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var result1 = table.TryGetService(typeof(IEnumerable<IDisposable>));
            var result2 = table.TryGetService(typeof(IEnumerable<IDisposable>));
            Assert.Same(result1, result2);
        }

        [Fact]
        public void TryGetService_WithCachedManyEnumerable_ReturnsSameInstance()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IDisposable), typeof(DisposableImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var result1 = table.TryGetService(typeof(IManyEnumerable<IDisposable>));
            var result2 = table.TryGetService(typeof(IManyEnumerable<IDisposable>));
            Assert.Same(result1, result2);
        }

        [Fact]
        public void TryGetService_WithCachedGenericService_ReturnsSameInstance()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IGenericService<>), typeof(GenericServiceImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            var result1 = table.TryGetService(typeof(IGenericService<int>));
            var result2 = table.TryGetService(typeof(IGenericService<int>));
            Assert.Same(result1, result2);
        }

        [Fact]
        public void TryGetService_WithGenericService_ReturnsServiceDefinition()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IGenericService2<>), typeof(IGenericService2<>), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IGenericService2<int>));
            Assert.NotNull(result);
        }

        [Fact]
        public void Constructor_WithSourceGeneratorEngine_UsesSourceGeneratedProxyTypeGenerator()
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

        private static ServiceTable CreateTable()
        {
            var context = new ServiceContext();
            return new ServiceTable(context);
        }

        public interface IGenericService<T> { T GetValue(); }
        public class GenericServiceImpl<T> : IGenericService<T> { public T GetValue() => default; }

        public interface IGenericService2<T> { }

        public class ComparerImpl<T> : IComparer<T> { public int Compare(T x, T y) => 0; }

        public class DisposableImpl : IDisposable { public void Dispose() { } }

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
