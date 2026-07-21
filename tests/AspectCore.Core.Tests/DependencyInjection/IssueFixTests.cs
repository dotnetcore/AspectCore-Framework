using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AspectCore.Configuration;
using AspectCore.DependencyInjection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    /// <summary>
    /// Tests for GitHub Issues #331, #271, and #254.
    /// </summary>
    public class IssueFixTests
    {
        #region Issue #331: ServiceTable proxy registration not overwritten by later non-proxy registration

        [Fact]
        public void Issue331_ProxyRegistration_NotOverwrittenByLaterNonProxyRegistration()
        {
            // Arrange: register service that will be proxied (has interceptor attribute)
            // then register same service type again without proxy
            var table = CreateTable();
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IInterceptedService), typeof(InterceptedServiceImpl), Lifetime.Transient)
            };
            table.Populate(services);

            // First registration should produce a ProxyServiceDefinition
            var firstResult = table.TryGetService(typeof(IInterceptedService));
            Assert.IsType<ProxyServiceDefinition>(firstResult);

            // Now simulate extra registration that adds a plain TypeServiceDefinition
            var extraServices = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IInterceptedService), typeof(InterceptedServiceImpl), Lifetime.Transient)
            };
            table.Populate(extraServices);

            // After extra registration, resolve should still prefer the ProxyServiceDefinition
            var result = table.TryGetService(typeof(IInterceptedService));
            Assert.NotNull(result);
            Assert.IsType<ProxyServiceDefinition>(result);
        }

        [Fact]
        public void Issue331_ProxyRegistration_PreferredRegardlessOfOrder()
        {
            // Arrange: register two definitions for the same type where one is proxy-able
            var context = new ServiceContext();
            // Add a configuration that intercepts IInterceptedService
            context.Configuration.Interceptors.AddTyped<TestIssueInterceptor>(
                Predicates.ForService("*IInterceptedService*"));

            var table = new ServiceTable(context);

            // First: a plain impl that won't be proxied (non-interface match)
            // Then: one that will be proxied
            var services = new List<ServiceDefinition>
            {
                new TypeServiceDefinition(typeof(IInterceptedService), typeof(InterceptedServiceImpl), Lifetime.Transient),
                // Add a InstanceServiceDefinition (which won't get proxied if there's no interceptor on its type)
                new InstanceServiceDefinition(typeof(IInterceptedService), new InterceptedServiceImpl())
            };
            table.Populate(services);

            // The proxy definition should be preferred
            var result = table.TryGetService(typeof(IInterceptedService));
            Assert.NotNull(result);
            Assert.IsType<ProxyServiceDefinition>(result);
        }

        [Fact]
        public void Issue331_ResolveFromServiceResolver_AfterExtraRegistration_StillReturnsProxy()
        {
            // Full integration test: use ServiceContext + ServiceResolver
            var context = new ServiceContext();
            context.Configuration.Interceptors.AddTyped<TestIssueInterceptor>(
                Predicates.ForService("*IInterceptedService*"));
            context.Transients.AddType<IInterceptedService, InterceptedServiceImpl>();
            // Simulate extra registration
            context.Add(new TypeServiceDefinition(typeof(IInterceptedService), typeof(InterceptedServiceImpl), Lifetime.Transient));

            var resolver = new ServiceResolver(context);
            var result = resolver.Resolve(typeof(IInterceptedService));

            Assert.NotNull(result);
            // The resolved instance type should be a proxy type, not the plain implementation
            Assert.NotEqual(typeof(InterceptedServiceImpl), result.GetType());
        }

        #endregion

        #region Issue #271: Singleton concurrent construction returns same instance

        [Fact]
        public void Issue271_Singleton_ConcurrentResolution_ReturnsSameInstance()
        {
            // Arrange: a singleton service with a slow constructor
            var context = new ServiceContext();
            var constructionCount = 0;
            context.Add(new DelegateServiceDefinition(
                typeof(ISlowSingletonService),
                resolver =>
                {
                    Interlocked.Increment(ref constructionCount);
                    Thread.Sleep(50); // Simulate slow construction
                    return new SlowSingletonServiceImpl();
                },
                Lifetime.Singleton));

            var resolver = new ServiceResolver(context);
            var results = new object[10];
            var barrier = new Barrier(10);

            // Act: resolve concurrently from multiple threads
            var tasks = new Task[10];
            for (var i = 0; i < 10; i++)
            {
                var index = i;
                tasks[i] = Task.Run(() =>
                {
                    barrier.SignalAndWait();
                    results[index] = resolver.Resolve(typeof(ISlowSingletonService));
                });
            }
            Task.WaitAll(tasks);

            // Assert: all threads should get the same instance
            var firstResult = results[0];
            Assert.NotNull(firstResult);
            for (var i = 1; i < results.Length; i++)
            {
                Assert.Same(firstResult, results[i]);
            }

            // The factory should have been called exactly once
            Assert.Equal(1, constructionCount);
        }

        [Fact]
        public void Issue271_Singleton_SequentialResolution_ReturnsSameInstance()
        {
            var context = new ServiceContext();
            context.Add(new TypeServiceDefinition(typeof(ISlowSingletonService), typeof(SlowSingletonServiceImpl), Lifetime.Singleton));
            var resolver = new ServiceResolver(context);

            var result1 = resolver.Resolve(typeof(ISlowSingletonService));
            var result2 = resolver.Resolve(typeof(ISlowSingletonService));

            Assert.Same(result1, result2);
        }

        [Fact]
        public void Issue271_Singleton_ChildResolver_SharesSameInstance()
        {
            var context = new ServiceContext();
            var constructionCount = 0;
            context.Add(new DelegateServiceDefinition(
                typeof(ISlowSingletonService),
                resolver =>
                {
                    Interlocked.Increment(ref constructionCount);
                    return new SlowSingletonServiceImpl();
                },
                Lifetime.Singleton));

            var rootResolver = new ServiceResolver(context);
            var childResolver = new ServiceResolver(rootResolver);

            var rootResult = rootResolver.Resolve(typeof(ISlowSingletonService));
            var childResult = childResolver.Resolve(typeof(ISlowSingletonService));

            Assert.Same(rootResult, childResult);
            Assert.Equal(1, constructionCount);
        }

        #endregion

        #region Issue #254: ProxyActivatorUtilities.CreateProxyInstance

        [Fact]
        public void Issue254_CreateProxyInstance_Generic_CreatesInterceptableProxy()
        {
            // Arrange: create a service provider with proxy generator
            var context = new ServiceContext();
            context.Configuration.Interceptors.AddTyped<TestIssueInterceptor>(
                Predicates.ForService("*ProxiableClass*"));
            var resolver = new ServiceResolver(context);

            // Act: create a proxy instance with runtime constructor arguments
            var proxy = ProxyActivatorUtilities.CreateProxyInstance<ProxiableClass>(resolver, "test-arg");

            // Assert
            Assert.NotNull(proxy);
            Assert.NotEqual(typeof(ProxiableClass), proxy.GetType());
            Assert.Equal("test-arg", proxy.Name);
        }

        [Fact]
        public void Issue254_CreateProxyInstance_NonGeneric_CreatesInterceptableProxy()
        {
            var context = new ServiceContext();
            context.Configuration.Interceptors.AddTyped<TestIssueInterceptor>(
                Predicates.ForService("*ProxiableClass*"));
            var resolver = new ServiceResolver(context);

            var proxy = ProxyActivatorUtilities.CreateProxyInstance(resolver, typeof(ProxiableClass), "test-arg");

            Assert.NotNull(proxy);
            Assert.IsAssignableFrom<ProxiableClass>(proxy);
            Assert.NotEqual(typeof(ProxiableClass), proxy.GetType());
            Assert.Equal("test-arg", ((ProxiableClass)proxy).Name);
        }

        [Fact]
        public void Issue254_CreateProxyInstance_ExtensionMethod_Works()
        {
            var context = new ServiceContext();
            context.Configuration.Interceptors.AddTyped<TestIssueInterceptor>(
                Predicates.ForService("*ProxiableClass*"));
            var resolver = new ServiceResolver(context);
            IServiceProvider sp = resolver;

            var proxy = sp.CreateProxyInstance<ProxiableClass>("hello");

            Assert.NotNull(proxy);
            Assert.NotEqual(typeof(ProxiableClass), proxy.GetType());
            Assert.Equal("hello", proxy.Name);
        }

        [Fact]
        public void Issue254_CreateProxyInstance_NullServiceProvider_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                ProxyActivatorUtilities.CreateProxyInstance<ProxiableClass>(null, "arg"));
        }

        [Fact]
        public void Issue254_CreateProxyInstance_NullServiceType_Throws()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            Assert.Throws<ArgumentNullException>(() =>
                ProxyActivatorUtilities.CreateProxyInstance(resolver, null, "arg"));
        }

        [Fact]
        public void Issue254_CreateProxyInstance_NullParameters_Throws()
        {
            var context = new ServiceContext();
            var resolver = new ServiceResolver(context);
            Assert.Throws<ArgumentNullException>(() =>
                ProxyActivatorUtilities.CreateProxyInstance<ProxiableClass>(resolver, null));
        }

        #endregion

        #region Test Types

        [TestIssueInterceptor]
        public interface IInterceptedService
        {
            string GetValue();
        }

        public class InterceptedServiceImpl : IInterceptedService
        {
            public string GetValue() => "original";
        }

        public interface ISlowSingletonService
        {
            int Id { get; }
        }

        public class SlowSingletonServiceImpl : ISlowSingletonService
        {
            private static int _nextId;
            public int Id { get; } = Interlocked.Increment(ref _nextId);
        }

        public class ProxiableClass
        {
            public ProxiableClass(string name)
            {
                Name = name;
            }

            public string Name { get; }

            [TestIssueInterceptor]
            public virtual string GetGreeting() => $"Hello, {Name}";
        }

        public class TestIssueInterceptor : AbstractInterceptorAttribute
        {
            public override Task Invoke(AspectContext context, AspectDelegate next)
            {
                return next(context);
            }
        }

        #endregion

        private static ServiceTable CreateTable()
        {
            // Create a ServiceContext with configuration that will proxy IInterceptedService
            var context = new ServiceContext();
            context.Configuration.Interceptors.AddTyped<TestIssueInterceptor>(
                Predicates.ForService("*IInterceptedService*"));
            return new ServiceTable(context);
        }
    }
}
