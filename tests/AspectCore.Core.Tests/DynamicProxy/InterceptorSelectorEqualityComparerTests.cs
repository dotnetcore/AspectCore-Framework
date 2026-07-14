using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AspectCore.DynamicProxy;
using Xunit;

namespace AspectCore.Core.Tests.DynamicProxy
{
    public class InterceptorSelectorEqualityComparerTests
    {
        private readonly InterceptorSelectorEqualityComparer<IInterceptorSelector> _comparer
            = new InterceptorSelectorEqualityComparer<IInterceptorSelector>();

        [Fact]
        public void Equals_SameReference_ReturnsTrue()
        {
            var selector = new FakeInterceptorSelector();
            Assert.True(_comparer.Equals(selector, selector));
        }

        [Fact]
        public void Equals_SameTypeDifferentInstances_ReturnsTrue()
        {
            var x = new FakeInterceptorSelector();
            var y = new FakeInterceptorSelector();
            Assert.True(_comparer.Equals(x, y));
        }

        [Fact]
        public void Equals_DifferentTypes_ReturnsFalse()
        {
            var x = new FakeInterceptorSelector();
            var y = new AnotherFakeInterceptorSelector();
            Assert.False(_comparer.Equals(x, y));
        }

        [Fact]
        public void Equals_FirstNull_ReturnsFalse()
        {
            var y = new FakeInterceptorSelector();
            Assert.False(_comparer.Equals(null, y));
        }

        [Fact]
        public void Equals_SecondNull_ReturnsFalse()
        {
            var x = new FakeInterceptorSelector();
            Assert.False(_comparer.Equals(x, null));
        }

        [Fact]
        public void Equals_BothNull_ReturnsFalse()
        {
            Assert.False(_comparer.Equals(null, null));
        }

        [Fact]
        public void GetHashCode_SameType_ReturnsConsistentHash()
        {
            var x = new FakeInterceptorSelector();
            var y = new FakeInterceptorSelector();
            Assert.Equal(_comparer.GetHashCode(x), _comparer.GetHashCode(y));
        }

        [Fact]
        public void GetHashCode_DifferentTypes_ReturnsDifferentHash()
        {
            var x = new FakeInterceptorSelector();
            var y = new AnotherFakeInterceptorSelector();
            Assert.NotEqual(_comparer.GetHashCode(x), _comparer.GetHashCode(y));
        }

        [Fact]
        public void GetHashCode_SameInstance_ReturnsStableHash()
        {
            var x = new FakeInterceptorSelector();
            Assert.Equal(_comparer.GetHashCode(x), _comparer.GetHashCode(x));
        }

        [Fact]
        public void GetHashCode_NullInput_Throws()
        {
            Assert.Throws<System.NullReferenceException>(() => _comparer.GetHashCode(null));
        }

        [Fact]
        public void Equals_UsedWithDistinct_RemovesDuplicateSelectorsByType()
        {
            var selectors = new List<IInterceptorSelector>
            {
                new FakeInterceptorSelector(),
                new FakeInterceptorSelector(),
                new AnotherFakeInterceptorSelector(),
                new AnotherFakeInterceptorSelector(),
            };

            var distinct = new List<IInterceptorSelector>(selectors.Distinct(_comparer));

            Assert.Equal(2, distinct.Count);
        }

        [Fact]
        public void Equals_UsedWithHashSet_ContainsOnlyUniqueTypes()
        {
            var set = new HashSet<IInterceptorSelector>(_comparer);
            set.Add(new FakeInterceptorSelector());
            set.Add(new FakeInterceptorSelector());
            set.Add(new AnotherFakeInterceptorSelector());

            Assert.Equal(2, set.Count);
            Assert.Contains(set, s => s is FakeInterceptorSelector);
            Assert.Contains(set, s => s is AnotherFakeInterceptorSelector);
        }

        private sealed class FakeInterceptorSelector : IInterceptorSelector
        {
            public IEnumerable<IInterceptor> Select(MethodInfo method)
            {
                yield break;
            }
        }

        private sealed class AnotherFakeInterceptorSelector : IInterceptorSelector
        {
            public IEnumerable<IInterceptor> Select(MethodInfo method)
            {
                yield break;
            }
        }
    }
}
