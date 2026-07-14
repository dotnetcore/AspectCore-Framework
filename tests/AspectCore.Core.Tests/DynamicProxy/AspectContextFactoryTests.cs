using System;
using System.Reflection;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectContextFactoryTests
    {
        public class TestService
        {
            public virtual int Add(int a, int b) => a + b;

            public virtual string GetName() => "test";

            public virtual Task<int> GetAsync() => Task.FromResult(42);

            public virtual void VoidMethod() { }
        }

        private static MethodInfo GetMethod(string name) => typeof(TestService).GetMethod(name);

        private static AspectActivatorContext CreateActivatorContext(
            MethodInfo serviceMethod = null,
            MethodInfo targetMethod = null,
            MethodInfo proxyMethod = null,
            MethodInfo predicateMethod = null,
            object targetInstance = null,
            object proxyInstance = null,
            object[] parameters = null)
        {
            var defaultMethod = GetMethod(nameof(TestService.Add));
            return new AspectActivatorContext(
                serviceMethod ?? defaultMethod,
                targetMethod ?? defaultMethod,
                proxyMethod ?? defaultMethod,
                predicateMethod ?? defaultMethod,
                targetInstance ?? new TestService(),
                proxyInstance ?? new TestService(),
                parameters);
        }

        private class FakeServiceProvider : IServiceProvider
        {
            public object GetService(Type serviceType) => null;
        }

        private class FakeDisposable : IDisposable
        {
            public bool Disposed { get; private set; }

            public void Dispose()
            {
                Disposed = true;
            }
        }

        // ---------- Constructor ----------

        [Fact]
        public void Constructor_NullServiceProvider_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new AspectContextFactory(null));
        }

        [Fact]
        public void Constructor_ValidServiceProvider_DoesNotThrow()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            Assert.NotNull(factory);
        }

        // ---------- CreateContext ----------

        [Fact]
        public void CreateContext_ReturnsRuntimeAspectContext()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var activatorContext = CreateActivatorContext();

            var context = factory.CreateContext(activatorContext);

            Assert.NotNull(context);
            Assert.IsType<RuntimeAspectContext>(context);
        }

        [Fact]
        public void CreateContext_SetsServiceProvider()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var activatorContext = CreateActivatorContext();

            var context = factory.CreateContext(activatorContext);

            Assert.Same(serviceProvider, context.ServiceProvider);
        }

        [Fact]
        public void CreateContext_SetsServiceMethod()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var method = GetMethod(nameof(TestService.Add));
            var activatorContext = CreateActivatorContext(serviceMethod: method);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(method, context.ServiceMethod);
        }

        [Fact]
        public void CreateContext_SetsImplementationMethod()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var method = GetMethod(nameof(TestService.GetName));
            var activatorContext = CreateActivatorContext(targetMethod: method);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(method, context.ImplementationMethod);
        }

        [Fact]
        public void CreateContext_SetsProxyMethod()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var method = GetMethod(nameof(TestService.GetAsync));
            var activatorContext = CreateActivatorContext(proxyMethod: method);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(method, context.ProxyMethod);
        }

        [Fact]
        public void CreateContext_SetsPredicateMethod()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var method = GetMethod(nameof(TestService.VoidMethod));
            var activatorContext = CreateActivatorContext(predicateMethod: method);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(method, context.PredicateMethod);
        }

        [Fact]
        public void CreateContext_SetsImplementation()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var target = new TestService();
            var activatorContext = CreateActivatorContext(targetInstance: target);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(target, context.Implementation);
        }

        [Fact]
        public void CreateContext_SetsProxy()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var proxy = new TestService();
            var activatorContext = CreateActivatorContext(proxyInstance: proxy);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(proxy, context.Proxy);
        }

        [Fact]
        public void CreateContext_SetsParameters()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var parameters = new object[] { 1, 2 };
            var activatorContext = CreateActivatorContext(parameters: parameters);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(parameters, context.Parameters);
            Assert.Equal(1, context.Parameters[0]);
            Assert.Equal(2, context.Parameters[1]);
        }

        [Fact]
        public void CreateContext_NullParameters_UsesEmptyArray()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var activatorContext = CreateActivatorContext(parameters: null);

            var context = factory.CreateContext(activatorContext);

            Assert.NotNull(context.Parameters);
            Assert.Empty(context.Parameters);
        }

        [Fact]
        public void CreateContext_AllPropertiesSet_MapsAllCorrectly()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var serviceMethod = GetMethod(nameof(TestService.Add));
            var targetMethod = GetMethod(nameof(TestService.GetName));
            var proxyMethod = GetMethod(nameof(TestService.GetAsync));
            var predicateMethod = GetMethod(nameof(TestService.VoidMethod));
            var target = new TestService();
            var proxy = new TestService();
            var parameters = new object[] { 10, 20 };

            var activatorContext = new AspectActivatorContext(
                serviceMethod, targetMethod, proxyMethod, predicateMethod,
                target, proxy, parameters);

            var context = factory.CreateContext(activatorContext);

            Assert.Same(serviceMethod, context.ServiceMethod);
            Assert.Same(targetMethod, context.ImplementationMethod);
            Assert.Same(proxyMethod, context.ProxyMethod);
            Assert.Same(predicateMethod, context.PredicateMethod);
            Assert.Same(target, context.Implementation);
            Assert.Same(proxy, context.Proxy);
            Assert.Same(parameters, context.Parameters);
        }

        // ---------- ReleaseContext ----------

        [Fact]
        public void ReleaseContext_DisposableContext_DisposesContext()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var activatorContext = CreateActivatorContext();
            var context = factory.CreateContext(activatorContext);
            context.AdditionalData["key"] = new FakeDisposable();

            factory.ReleaseContext(context);

            // After release, the context should be disposed; AdditionalData should be cleared.
            Assert.Empty(context.AdditionalData);
        }

        [Fact]
        public void ReleaseContext_DisposableContext_DisposesDisposableValues()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var activatorContext = CreateActivatorContext();
            var context = factory.CreateContext(activatorContext);
            var disposable = new FakeDisposable();
            context.AdditionalData["disposable"] = disposable;

            factory.ReleaseContext(context);

            Assert.True(disposable.Disposed);
        }

        [Fact]
        public void ReleaseContext_NullContext_DoesNotThrow()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);

            // Should not throw when context is null (cast to IDisposable yields null).
            factory.ReleaseContext(null);
        }

        [Fact]
        public void ReleaseContext_ContextWithoutAdditionalData_DoesNotThrow()
        {
            var serviceProvider = new FakeServiceProvider();
            var factory = new AspectContextFactory(serviceProvider);
            var activatorContext = CreateActivatorContext();
            var context = factory.CreateContext(activatorContext);

            // Don't access AdditionalData so _data remains null.
            factory.ReleaseContext(context);
        }
    }
}
