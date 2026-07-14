using System;
using System.Collections;
using System.Collections.Generic;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceProviderExtensionsTests
    {
        #region Resolve<T>

        [Fact]
        public void Resolve_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            IServiceProvider serviceProvider = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceProvider.Resolve<object>());
            Assert.Equal("serviceProvider", ex.ParamName);
        }

        [Fact]
        public void Resolve_WhenProviderIsServiceResolver_CallsResolveDirectly()
        {
            var expected = new object();
            var resolver = new FakeServiceResolver(expected);
            var result = resolver.Resolve<object>();
            Assert.Same(expected, result);
        }

        [Fact]
        public void Resolve_WhenProviderIsNotServiceResolver_ResolvesViaGetService()
        {
            var expected = new object();
            var innerResolver = new FakeServiceResolver(expected);
            var provider = new FakeServiceProvider(innerResolver);
            var result = provider.Resolve<object>();
            Assert.Same(expected, result);
        }


        [Fact]
        public void Resolve_WhenProviderTypedAsIServiceProviderButIsResolver_UsesResolverPath()
        {
            var expected = new object();
            IServiceProvider provider = new FakeServiceResolver(expected);
            var result = provider.Resolve<object>();
            Assert.Same(expected, result);
        }

        #endregion

        #region ResolveRequired<T>

        [Fact]
        public void ResolveRequired_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            IServiceProvider serviceProvider = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceProvider.ResolveRequired<object>());
            Assert.Equal("serviceProvider", ex.ParamName);
        }

        [Fact]
        public void ResolveRequired_WhenProviderIsServiceResolver_CallsResolveRequiredDirectly()
        {
            var expected = new object();
            var resolver = new FakeServiceResolver(expected);
            var result = resolver.ResolveRequired<object>();
            Assert.Same(expected, result);
        }

        [Fact]
        public void ResolveRequired_WhenProviderIsNotServiceResolver_ResolvesViaGetService()
        {
            var expected = new object();
            var innerResolver = new FakeServiceResolver(expected);
            var provider = new FakeServiceProvider(innerResolver);
            var result = provider.ResolveRequired<object>();
            Assert.Same(expected, result);
        }

        [Fact]
        public void ResolveRequired_WhenProviderTypedAsIServiceProviderButIsResolver_UsesResolverPath()
        {
            var expected = new object();
            IServiceProvider provider = new FakeServiceResolver(expected);
            var result = provider.ResolveRequired<object>();
            Assert.Same(expected, result);
        }

        #endregion

        #region ResolveMany<T>

        [Fact]
        public void ResolveMany_WithNullServiceProvider_ThrowsArgumentNullException()
        {
            IServiceProvider serviceProvider = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceProvider.ResolveMany<object>());
            Assert.Equal("serviceProvider", ex.ParamName);
        }

        [Fact]
        public void ResolveMany_WhenProviderIsServiceResolver_CallsResolveManyDirectly()
        {
            var items = new List<string> { "a", "b" };
            var resolver = new FakeServiceResolver(null, items);
            var result = resolver.ResolveMany<string>();
            Assert.Equal(items, result);
        }

        [Fact]
        public void ResolveMany_WhenProviderIsNotServiceResolver_ResolvesViaGetService()
        {
            var items = new List<string> { "x", "y" };
            var innerResolver = new FakeServiceResolver(null, items);
            var provider = new FakeServiceProvider(innerResolver);
            var result = provider.ResolveMany<string>();
            Assert.Equal(items, result);
        }

        [Fact]
        public void ResolveMany_WhenProviderTypedAsIServiceProviderButIsResolver_UsesResolverPath()
        {
            var items = new List<string> { "a", "b" };
            IServiceProvider provider = new FakeServiceResolver(null, items);
            var result = provider.ResolveMany<string>();
            Assert.Equal(items, result);
        }

        #endregion

        #region Test Types

        private class FakeManyEnumerable<T> : IManyEnumerable<T>
        {
            private readonly IEnumerable<T> _items;
            public FakeManyEnumerable(IEnumerable<T> items) => _items = items;
            public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// <summary>
        /// Fake IServiceResolver that returns a predefined object for Resolve,
        /// wraps a predefined enumerable for IManyEnumerable requests, and a fake
        /// factory for IScopeResolverFactory.
        /// </summary>
        private class FakeServiceResolver : IServiceResolver
        {
            private readonly object _resolveResult;
            private readonly IEnumerable _resolveManyResult;

            public FakeServiceResolver(object resolveResult, IEnumerable resolveManyResult = null)
            {
                _resolveResult = resolveResult;
                _resolveManyResult = resolveManyResult;
            }

            public object Resolve(Type serviceType)
            {
                if (serviceType == typeof(IScopeResolverFactory))
                {
                    return new FakeScopeResolverFactory();
                }
                if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof(IManyEnumerable<>))
                {
                    var itemType = serviceType.GetGenericArguments()[0];
                    var wrapperType = typeof(FakeManyEnumerable<>).MakeGenericType(itemType);
                    return Activator.CreateInstance(wrapperType, _resolveManyResult);
                }
                return _resolveResult;
            }

            public object GetService(Type serviceType) => Resolve(serviceType);

            public object GetKeyedService(Type serviceType, object serviceKey) => Resolve(serviceType);

            public object GetRequiredKeyedService(Type serviceType, object serviceKey) => Resolve(serviceType);

            public void Dispose() { }

            private class FakeScopeResolverFactory : IScopeResolverFactory
            {
                public IServiceResolver CreateScope() => new FakeServiceResolver(null);
            }
        }

        /// <summary>
        /// Fake IServiceProvider that is NOT an IServiceResolver. It resolves
        /// IServiceResolver via GetService so the extension methods can use the
        /// fallback path.
        /// </summary>
        private class FakeServiceProvider : IServiceProvider
        {
            private readonly IServiceResolver _resolver;

            public FakeServiceProvider(IServiceResolver resolver) => _resolver = resolver;

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IServiceResolver))
                {
                    return _resolver;
                }
                return null;
            }
        }

        #endregion
    }
}
