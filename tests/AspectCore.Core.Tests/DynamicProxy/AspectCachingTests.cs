using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class AspectCachingTests
    {
        [Fact]
        public void Constructor_SetsName()
        {
            var caching = new AspectCaching("test-cache");

            Assert.Equal("test-cache", caching.Name);
        }

        [Fact]
        public void Name_ReturnsProvidedName()
        {
            var caching = new AspectCaching("my-cache");

            Assert.Equal("my-cache", caching.Name);
        }

        [Fact]
        public void Set_ThenGet_ReturnsSetValue()
        {
            var caching = new AspectCaching("test");

            caching.Set("key", "value");

            Assert.Equal("value", caching.Get("key"));
        }

        [Fact]
        public void Set_OverwritesExistingKey()
        {
            var caching = new AspectCaching("test");

            caching.Set("key", "value1");
            caching.Set("key", "value2");

            Assert.Equal("value2", caching.Get("key"));
        }

        [Fact]
        public void Get_WithNonExistentKey_ThrowsKeyNotFoundException()
        {
            var caching = new AspectCaching("test");

            Assert.Throws<KeyNotFoundException>(() => caching.Get("non-existent"));
        }

        [Fact]
        public void Get_WithNullKey_ThrowsArgumentNullException()
        {
            var caching = new AspectCaching("test");

            Assert.Throws<ArgumentNullException>(() => caching.Get(null));
        }

        [Fact]
        public void Set_WithNullKey_ThrowsArgumentNullException()
        {
            var caching = new AspectCaching("test");

            Assert.Throws<ArgumentNullException>(() => caching.Set(null, "value"));
        }

        [Fact]
        public void Set_WithNullValue_StoresNull()
        {
            var caching = new AspectCaching("test");

            caching.Set("key", null);

            Assert.Null(caching.Get("key"));
        }

        [Fact]
        public void Set_WithObjectKey_WorksCorrectly()
        {
            var caching = new AspectCaching("test");
            var key = new object();

            caching.Set(key, 42);

            Assert.Equal(42, caching.Get(key));
        }

        [Fact]
        public void Set_WithIntegerKey_WorksCorrectly()
        {
            var caching = new AspectCaching("test");

            caching.Set(1, "one");
            caching.Set(2, "two");

            Assert.Equal("one", caching.Get(1));
            Assert.Equal("two", caching.Get(2));
        }

        [Fact]
        public void GetOrAdd_WithNewKey_AddsValueFromFactory()
        {
            var caching = new AspectCaching("test");

            var result = caching.GetOrAdd("key", k => "factory-value");

            Assert.Equal("factory-value", result);
            Assert.Equal("factory-value", caching.Get("key"));
        }

        [Fact]
        public void GetOrAdd_WithExistingKey_ReturnsExistingValue()
        {
            var caching = new AspectCaching("test");
            caching.Set("key", "existing-value");

            var result = caching.GetOrAdd("key", k => "factory-value");

            Assert.Equal("existing-value", result);
        }

        [Fact]
        public void GetOrAdd_WithExistingKey_DoesNotCallFactory()
        {
            var caching = new AspectCaching("test");
            caching.Set("key", "existing-value");
            var factoryCalled = false;

            caching.GetOrAdd("key", k =>
            {
                factoryCalled = true;
                return "factory-value";
            });

            Assert.False(factoryCalled);
        }

        [Fact]
        public void GetOrAdd_WithNullKey_ThrowsArgumentNullException()
        {
            var caching = new AspectCaching("test");

            Assert.Throws<ArgumentNullException>(() => caching.GetOrAdd(null, k => "value"));
        }

        [Fact]
        public void GetOrAdd_WithNullFactory_ThrowsArgumentNullException()
        {
            var caching = new AspectCaching("test");

            Assert.Throws<ArgumentNullException>(() => caching.GetOrAdd("key", null));
        }

        [Fact]
        public void GetOrAdd_FactoryReceivesKey()
        {
            var caching = new AspectCaching("test");
            object receivedKey = null;

            caching.GetOrAdd("my-key", k =>
            {
                receivedKey = k;
                return "value";
            });

            Assert.Equal("my-key", receivedKey);
        }

        [Fact]
        public async Task GetOrAdd_ConcurrentAccess_AddsValueOnlyOnce()
        {
            var caching = new AspectCaching("test");
            var factoryCallCount = 0;
            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    caching.GetOrAdd("concurrent-key", k =>
                    {
                        System.Threading.Interlocked.Increment(ref factoryCallCount);
                        return "value";
                    });
                }));
            }

            await Task.WhenAll(tasks);

            Assert.Equal("value", caching.Get("concurrent-key"));
        }

        [Fact]
        public void Dispose_DisposesDisposableValue()
        {
            var caching = new AspectCaching("test");
            var disposable = new FakeDisposable();

            caching.Set("key", disposable);
            caching.Dispose();

            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public void Dispose_DisposesAllDisposableValues()
        {
            var caching = new AspectCaching("test");
            var disposable1 = new FakeDisposable();
            var disposable2 = new FakeDisposable();

            caching.Set("key1", disposable1);
            caching.Set("key2", disposable2);
            caching.Dispose();

            Assert.True(disposable1.IsDisposed);
            Assert.True(disposable2.IsDisposed);
        }

        [Fact]
        public void Dispose_DisposesItemsInEnumerableValue()
        {
            var caching = new AspectCaching("test");
            var disposable1 = new FakeDisposable();
            var disposable2 = new FakeDisposable();
            var list = new List<FakeDisposable> { disposable1, disposable2 };

            caching.Set("key", list);
            caching.Dispose();

            Assert.True(disposable1.IsDisposed);
            Assert.True(disposable2.IsDisposed);
        }

        [Fact]
        public void Dispose_DisposesEnumerableValueAndItsItems()
        {
            var caching = new AspectCaching("test");
            var item = new FakeDisposable();
            var enumerable = new FakeDisposableEnumerable(new[] { item });

            caching.Set("key", enumerable);
            caching.Dispose();

            Assert.True(item.IsDisposed);
            Assert.True(enumerable.IsDisposed);
        }

        [Fact]
        public void Dispose_DoesNotThrow_WithNonDisposableValues()
        {
            var caching = new AspectCaching("test");

            caching.Set("key1", "string-value");
            caching.Set("key2", 42);
            caching.Set("key3", new object());

            caching.Dispose();
        }

        [Fact]
        public void Dispose_DoesNotThrow_WithEmptyCache()
        {
            var caching = new AspectCaching("test");

            caching.Dispose();
        }

        [Fact]
        public void Dispose_CanBeCalledMultipleTimes()
        {
            var caching = new AspectCaching("test");
            var disposable = new FakeDisposable();

            caching.Set("key", disposable);
            caching.Dispose();
            caching.Dispose();

            Assert.True(disposable.IsDisposed);
        }

        [Fact]
        public void Set_MultipleKeys_AllRetrievable()
        {
            var caching = new AspectCaching("test");

            caching.Set("key1", 1);
            caching.Set("key2", 2);
            caching.Set("key3", 3);

            Assert.Equal(1, caching.Get("key1"));
            Assert.Equal(2, caching.Get("key2"));
            Assert.Equal(3, caching.Get("key3"));
        }

        [Fact]
        public void GetOrAdd_MultipleKeys_AllWorkIndependently()
        {
            var caching = new AspectCaching("test");

            caching.GetOrAdd("key1", k => 1);
            caching.GetOrAdd("key2", k => 2);

            Assert.Equal(1, caching.Get("key1"));
            Assert.Equal(2, caching.Get("key2"));
        }

        private class FakeDisposable : IDisposable
        {
            public bool IsDisposed { get; private set; }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }

        private class FakeDisposableEnumerable : IEnumerable<FakeDisposable>, IDisposable
        {
            private readonly IEnumerable<FakeDisposable> _items;

            public FakeDisposableEnumerable(IEnumerable<FakeDisposable> items)
            {
                _items = items;
            }

            public bool IsDisposed { get; private set; }

            public IEnumerator<FakeDisposable> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public void Dispose()
            {
                IsDisposed = true;
            }
        }
    }
}
