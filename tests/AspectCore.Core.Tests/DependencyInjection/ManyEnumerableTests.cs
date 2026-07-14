using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AspectCore.DependencyInjection;
using Xunit;

namespace AspectCore.Core.Tests.DependencyInjection
{
    public class ManyEnumerableTests
    {
        [Fact]
        public void GetEnumerator_ReturnsAllItems()
        {
            var items = new[] { 1, 2, 3 };
            var many = new ManyEnumerable<int>(items);
            Assert.Equal(items, many.ToArray());
        }

        [Fact]
        public void GetEnumerator_NonGeneric_ReturnsAllItems()
        {
            var items = new[] { "a", "b", "c" };
            var many = new ManyEnumerable<string>(items);
            var result = new List<object>();
            var enumerator = ((IEnumerable)many).GetEnumerator();
            while (enumerator.MoveNext())
            {
                result.Add(enumerator.Current);
            }
            Assert.Equal(new object[] { "a", "b", "c" }, result);
        }

        [Fact]
        public void GetEnumerator_EmptyCollection_ReturnsEmpty()
        {
            var many = new ManyEnumerable<int>(new int[0]);
            Assert.Empty(many);
        }

        [Fact]
        public void GetEnumerator_WithListSource_ReturnsAllItems()
        {
            var items = new List<int> { 10, 20, 30 };
            var many = new ManyEnumerable<int>(items);
            Assert.Equal(items, many.ToArray());
        }
    }
}
