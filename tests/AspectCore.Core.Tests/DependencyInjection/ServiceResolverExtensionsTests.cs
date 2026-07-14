using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ServiceResolverExtensionsTests
    {
        #region Resolve<T>

        [Fact]
        public void Resolve_WithNullServiceResolver_ThrowsArgumentNullException()
        {
            IServiceResolver serviceResolver = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceResolver.Resolve<object>());
            Assert.Equal("serviceResolver", ex.ParamName);
        }

        [Fact]
        public void Resolve_ReturnsTypedInstance()
        {
            var expected = new object();
            var resolver = new FakeServiceResolver(expected);
            var result = resolver.Resolve<object>();
            Assert.Same(expected, result);
        }

        [Fact]
        public void Resolve_CastsToCorrectType()
        {
            var resolver = new FakeServiceResolver(42);
            var result = resolver.Resolve<int>();
            Assert.Equal(42, result);
        }

        #endregion

        #region CreateScope

        [Fact]
        public void CreateScope_WithNullServiceResolver_ThrowsArgumentNullException()
        {
            IServiceResolver serviceResolver = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceResolver.CreateScope());
            Assert.Equal("serviceResolver", ex.ParamName);
        }

        [Fact]
        public void CreateScope_ReturnsNewScopeFromFactory()
        {
            var scopeResolver = new FakeServiceResolver(null);
            var factory = new FakeScopeResolverFactory(scopeResolver);
            var resolver = new FakeServiceResolver(factory);
            var scope = resolver.CreateScope();
            Assert.Same(scopeResolver, scope);
        }

        #endregion

        #region ResolveRequired(Type)

        [Fact]
        public void ResolveRequired_Type_WithNullServiceResolver_ThrowsArgumentNullException()
        {
            IServiceResolver serviceResolver = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceResolver.ResolveRequired(typeof(object)));
            Assert.Equal("serviceResolver", ex.ParamName);
        }

        [Fact]
        public void ResolveRequired_Type_ReturnsInstanceWhenResolved()
        {
            var expected = new object();
            var resolver = new FakeServiceResolver(expected);
            var result = resolver.ResolveRequired(typeof(object));
            Assert.Same(expected, result);
        }

        [Fact]
        public void ResolveRequired_Type_WhenResolveReturnsNull_ThrowsInvalidOperationException()
        {
            var resolver = new FakeServiceResolver(null);
            var ex = Assert.Throws<InvalidOperationException>(() => resolver.ResolveRequired(typeof(object)));
            Assert.Contains("No instance for type", ex.Message);
            Assert.Contains("System.Object", ex.Message);
        }

        #endregion

        #region ResolveRequired<T>

        [Fact]
        public void ResolveRequired_Typed_WithNullServiceResolver_ThrowsArgumentNullException()
        {
            IServiceResolver serviceResolver = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceResolver.ResolveRequired<object>());
            Assert.Equal("serviceResolver", ex.ParamName);
        }

        [Fact]
        public void ResolveRequired_Typed_ReturnsTypedInstance()
        {
            var expected = "hello";
            var resolver = new FakeServiceResolver(expected);
            var result = resolver.ResolveRequired<string>();
            Assert.Equal(expected, result);
        }

        #endregion

        #region ResolveMany(Type)

        [Fact]
        public void ResolveMany_Type_WithNullServiceResolver_ThrowsArgumentNullException()
        {
            IServiceResolver serviceResolver = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceResolver.ResolveMany(typeof(object)));
            Assert.Equal("serviceResolver", ex.ParamName);
        }

        [Fact]
        public void ResolveMany_Type_WithNullServiceType_ThrowsArgumentNullException()
        {
            var resolver = new FakeServiceResolver(null);
            var ex = Assert.Throws<ArgumentNullException>(() => resolver.ResolveMany(null));
            Assert.Equal("serviceType", ex.ParamName);
        }

        [Fact]
        public void ResolveMany_Type_ReturnsEnumerableOfObjects()
        {
            var items = new List<object> { "a", 1, true };
            var resolver = new FakeServiceResolver(null, items);
            var result = resolver.ResolveMany(typeof(object));
            Assert.Equal(items, result.ToList());
        }

        #endregion

        #region ResolveMany<T>

        [Fact]
        public void ResolveMany_Typed_WithNullServiceResolver_ThrowsArgumentNullException()
        {
            IServiceResolver serviceResolver = null;
            var ex = Assert.Throws<ArgumentNullException>(() => serviceResolver.ResolveMany<object>());
            Assert.Equal("serviceResolver", ex.ParamName);
        }

        [Fact]
        public void ResolveMany_Typed_ReturnsTypedEnumerable()
        {
            var items = new List<string> { "a", "b", "c" };
            var resolver = new FakeServiceResolver(null, items);
            var result = resolver.ResolveMany<string>();
            Assert.Equal(items, result.ToList());
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

        private class FakeScopeResolverFactory : IScopeResolverFactory
        {
            private readonly IServiceResolver _scope;
            public FakeScopeResolverFactory(IServiceResolver scope) => _scope = scope;
            public IServiceResolver CreateScope() => _scope;
        }

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
                    return _resolveResult as IScopeResolverFactory ?? new FakeScopeResolverFactory(null);
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
        }

        #endregion
    }
}
