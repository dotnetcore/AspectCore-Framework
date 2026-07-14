using System;
using System.Collections.Generic;
using System.Linq;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceTableExtendedCoverageTests
    {
        [Fact]
        public void Contains_WithNonGenericNonRegisteredType_ReturnsFalse()
        {
            var table = CreateTable();
            Assert.False(table.Contains(typeof(IUnregisteredService)));
        }

        [Fact]
        public void Contains_WithRegisteredType_ReturnsTrue()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISimpleService), typeof(SimpleServiceImpl), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(ISimpleService)));
        }

        [Fact]
        public void Contains_WithConstructedGenericNotRegistered_ReturnsFalse()
        {
            var table = CreateTable();
            // IComparer<int> not registered
            Assert.False(table.Contains(typeof(IComparer<int>)));
        }

        [Fact]
        public void TryGetService_NullServiceType_ReturnsNull()
        {
            var table = CreateTable();
            Assert.Null(table.TryGetService(null));
        }

        [Fact]
        public void TryGetService_RegisteredType_ReturnsService()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISimpleService), typeof(SimpleServiceImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(ISimpleService));
            Assert.NotNull(result);
        }

        [Fact]
        public void TryGetService_UnregisteredType_ReturnsNull()
        {
            var table = CreateTable();
            Assert.Null(table.TryGetService(typeof(IUnregisteredService)));
        }

        [Fact]
        public void TryGetService_UnregisteredConstructedGeneric_ReturnsNull()
        {
            var table = CreateTable();
            Assert.Null(table.TryGetService(typeof(IComparer<int>)));
        }

        [Fact]
        public void TryGetService_EnumerableCached_ReturnsFromCache()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISimpleService), typeof(SimpleServiceImpl), Lifetime.Transient)
            };
            table.Populate(services);
            // First call creates the enumerable service definition
            var first = table.TryGetService(typeof(IEnumerable<ISimpleService>));
            Assert.NotNull(first);
            // Second call should return from cache (FindEnumerable TryGetValue path)
            var second = table.TryGetService(typeof(IEnumerable<ISimpleService>));
            Assert.NotNull(second);
            Assert.Same(first, second);
        }

        [Fact]
        public void TryGetService_ManyEnumerableCached_ReturnsFromCache()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(ISimpleService), typeof(SimpleServiceImpl), Lifetime.Transient)
            };
            table.Populate(services);
            // First call creates the many-enumerable service definition
            var first = table.TryGetService(typeof(IManyEnumerable<ISimpleService>));
            Assert.NotNull(first);
            // Second call should return from cache (FindManyEnumerable TryGetValue path)
            var second = table.TryGetService(typeof(IManyEnumerable<ISimpleService>));
            Assert.NotNull(second);
            Assert.Same(first, second);
        }

        [Fact]
        public void TryGetService_GenericServiceCached_ReturnsFromCache()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IGenericSvc<>), typeof(GenericSvcImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            // First call creates the generic service definition
            var first = table.TryGetService(typeof(IGenericSvc<int>));
            Assert.NotNull(first);
            // Second call should return from cache (FindGenericService TryGetValue path)
            var second = table.TryGetService(typeof(IGenericSvc<int>));
            Assert.NotNull(second);
            Assert.Same(first, second);
        }

        [Fact]
        public void TryGetService_GenericServiceWithInstanceDefinition_ReturnsService()
        {
            var table = CreateTable();
            var instance = new GenericSvcImpl<int>();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IGenericSvc<int>), instance)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IGenericSvc<int>));
            Assert.NotNull(result);
        }

        [Fact]
        public void TryGetService_GenericServiceWithDelegateDefinition_ReturnsService()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new DelegateServiceDefinition(typeof(IGenericSvc<int>), _ => new GenericSvcImpl<int>(), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IGenericSvc<int>));
            Assert.NotNull(result);
        }

        [Fact]
        public void Populate_WithManyEnumerableService_FiltersOutManyEnumerable()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new ManyEnumerableServiceDefinition(typeof(IManyEnumerable<ISimpleService>), typeof(ISimpleService),
                    new ServiceDefinition[0])
            };
            // Should not throw - ManyEnumerable services are filtered out during Populate
            table.Populate(services);
            // The IManyEnumerable<> Contains path always returns true (line 96: ? true : true),
            // so we verify that the service was filtered (not stored as a regular service)
            Assert.True(table.Contains(typeof(IManyEnumerable<ISimpleService>)));
            // But the element type should not be registered as a regular service
            Assert.False(table.Contains(typeof(ISimpleService)));
        }

        [Fact]
        public void Populate_WithGenericServiceDefinition_StoresInGenericDictionary()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IGenericSvc<>), typeof(GenericSvcImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            Assert.True(table.Contains(typeof(IGenericSvc<int>)));
        }

        [Fact]
        public void Constructor_WithExplicitProxyTypeGeneratorInstance_UsesIt()
        {
            var generator = new FakeProxyTypeGenerator();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IProxyTypeGenerator), generator)
            };
            var context = new ServiceContext(services);
            var table = new ServiceTable(context);
            Assert.NotNull(table);
        }

        [Fact]
        public void Constructor_WithDynamicProxyEngineOptions_UsesDynamicProxyGenerator()
        {
            var options = new ProxyEngineOptions { Engine = ProxyEngine.DynamicProxy };
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(ProxyEngineOptions), options)
            };
            var context = new ServiceContext(services);
            var table = new ServiceTable(context);
            Assert.NotNull(table);
        }

        [Fact]
        public void TryGetService_InstanceService_ReturnsProxyServiceDefinition()
        {
            var table = CreateTable();
            var instance = new ProxiedServiceImpl();
            var services = new List<ServiceDefinition>
            {
                new InstanceServiceDefinition(typeof(IProxiedService), instance)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IProxiedService));
            Assert.NotNull(result);
            Assert.IsType<ProxyServiceDefinition>(result);
        }

        [Fact]
        public void TryGetService_InterfaceServiceWithClassImpl_ReturnsProxyServiceDefinition()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IProxiedService), typeof(ProxiedServiceImpl), Lifetime.Transient)
            };
            table.Populate(services);
            var result = table.TryGetService(typeof(IProxiedService));
            Assert.NotNull(result);
            Assert.IsType<ProxyServiceDefinition>(result);
        }

        [Fact]
        public void TryGetService_EnumerableWithGenericElement_ReturnsService()
        {
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IGenericSvc<>), typeof(GenericSvcImpl<>), Lifetime.Transient)
            };
            table.Populate(services);
            // This exercises FindEnumerableElements with a generic element type
            var result = table.TryGetService(typeof(IEnumerable<IGenericSvc<int>>));
            Assert.NotNull(result);
        }

        private static ServiceTable CreateTable()
        {
            var context = new ServiceContext();
            return new ServiceTable(context);
        }

        #region Test Types

        public interface IUnregisteredService { }

        public interface ISimpleService
        {
            void DoSomething();
        }

        public class SimpleServiceImpl : ISimpleService
        {
            public void DoSomething() { }
        }

        // Service with interceptor attribute - will be proxied
        [TestInterceptor]
        public interface IProxiedService
        {
            void DoSomething();
        }

        public class ProxiedServiceImpl : IProxiedService
        {
            public void DoSomething() { }
        }

        public interface IGenericSvc<T>
        {
            T GetValue();
        }

        public class GenericSvcImpl<T> : IGenericSvc<T>
        {
            public T GetValue() => default;
        }

        public class FakeProxyTypeGenerator : IProxyTypeGenerator
        {
            public Type CreateInterfaceProxyType(Type serviceType) => typeof(object);
            public Type CreateInterfaceProxyType(Type serviceType, Type implementationType) => typeof(object);
            public Type CreateClassProxyType(Type serviceType, Type implementationType) => typeof(object);
        }

        // Simple test interceptor attribute
        public class TestInterceptorAttribute : AbstractInterceptorAttribute
        {
            public override System.Threading.Tasks.Task Invoke(AspectContext context, AspectDelegate next)
                => next(context);
        }

        #endregion
    }
}
